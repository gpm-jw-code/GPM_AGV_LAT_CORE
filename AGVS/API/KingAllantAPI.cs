using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public class KingAllantAPI : IAgvsHandShakeable, IDisposable
    {

        ILogger logger = new LoggerInstance(typeof(KingAllantAPI));
        private bool disposedValue;

        public TcpSocketClient socketClient { get; private set; }
        public bool connected { get; private set; } = false;
        public MessageHandShakeLogger mhsLogger { get; } = new MessageHandShakeLogger();

        public KingAllantAPI(TcpSocketClient socketClient)
        {
            this.socketClient = socketClient;
            connected = socketClient.socket.Connected;
            socketClient.OnDisconnect += SocketClient_OnDisconnect;
        }

        public void ReconnectDone(TcpSocketClient socketClient)
        {
            this.socketClient = socketClient;
            connected = true;
        }

        private void SocketClient_OnDisconnect(object sender, EventArgs e)
        {
            connected = false;
        }

        public async Task<bool> RunningStatusReport(Dictionary<string, object> agvcRunningStateData)
        {
            SocketStates states = await ReportRequestMessageSendOut(JsonConvert.SerializeObject(agvcRunningStateData), "0106");
            return states.receieveLen != 0;
        }


        public async Task ReportTaskDownloadResult(IAGVC agvc, bool success, IAGVSExecutingState executingState = null)
        {
            AgvcInfoForKingAllant agvcInfo = (AgvcInfoForKingAllant)agvc.agvcInfos;
            Dictionary<string, object> taskDownloadReply = CreateModelBase(agvc);
            taskDownloadReply["System Bytes"] = agvc.orderList_LAT.Last().latOrderDetail.action.actionIndex;
            taskDownloadReply["Header"] = new Dictionary<string, object>()
            {
                {"0302", new Dictionary<string, object>()
                {
                    { "Return Code",success?0:400 }
                }
                }
            };
            await AckMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply));
        }



        public void ReportNagivateResetExecuteResult(IAGVC agvc, bool success, IAGVSExecutingState executingState = null)
        {
            Dictionary<string, object> taskDownloadReply = CreateModelBase(agvc);
            taskDownloadReply["Header"] = new Dictionary<string, object>()
            {
                {"0306", new Dictionary<string, object>()
                {
                    { "Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    { "Return Code",success?0:400 },
                }
                }
            };
            AckMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply));

        }

        public void ReportNagivateTaskState(clsHostExecuting order)
        {
            var agvcInfo = order.ExecuteingAGVCInfo.agvcInfoForagvs as AgvcInfoForKingAllant;
            Dictionary<string, object> taskDownloadReply = CreateModelBase(agvcInfo);
            taskDownloadReply["Header"] = new Dictionary<string, object>()
            {
                 {"0303", new Dictionary<string, object>()
                        {
                            { "Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") }, //上報時間
                            { "Task Name", order.latOrderDetail.taskName },     //任務名稱
                            { "Task Simplex", string.Format("{0}_{1}",order.latOrderDetail.taskName ,order.latOrderDetail.action.actionIndex ) },  //任務TC拆解段落下給AGVC(??? 工蝦小)
                            { "Task Sequence", order.latOrderDetail.action.actionIndex }, //該路徑的第幾個點位
                            { "Task Status", GetTaskStatusFromLATState(order.State)},
                        }
                    }
            };
            ReportRequestMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply), "0304");
        }

        private Dictionary<string, object> CreateModelBase(AgvcInfoForKingAllant agvcInfo)
        {
            Dictionary<string, object> taskDownloadReply = new Dictionary<string, object>()
            {
                { "SID",agvcInfo.SID},
                { "EQName",agvcInfo.EQName},
                { "System Bytes",(uint)302},
                { "Header",new Dictionary<string, object>()}
            };
            return taskDownloadReply;
        }
        private Dictionary<string, object> CreateModelBase(IAGVC agvc)
        {
            AgvcInfoForKingAllant agvcInfo = (AgvcInfoForKingAllant)agvc.agvcInfos;
            return CreateModelBase(agvcInfo);
        }


        private int GetTaskStatusFromLATState(ORDER_STATE state)
        {
            switch (state)
            {
                case ORDER_STATE.WAIT_EXECUTE:
                    return 0;
                case ORDER_STATE.EXECUTING:
                    return 1;
                case ORDER_STATE.COMPLETE:
                    return 2;
                case ORDER_STATE.STOPPED:
                    return 4;
                case ORDER_STATE.FAILED:
                    return 1;
                case ORDER_STATE.ERROR:
                    return 4;
                default:
                    return 1;
            }
        }

        private async Task<bool> AckMessageSendOut(string json)
        {
            if (!connected)
            {
                logger.WarnLog($"無法發送 [Ack] 給kingGallent派車系統，因為socket連線已經中斷");
                return false;
            }
            mhsLogger.LATToAGVS(json);
            try
            {
                SocketStates ret = await SocketSendOut(json, false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("發送封包給派車時發生錯誤{0}", ex.Message);
                return false;
            }
        }


        private async Task<SocketStates> ReportRequestMessageSendOut(string requestJson, string check_match_str = null)
        {
            if (!connected)
            {
                logger.WarnLog($"無法發送 [Request] 給kingGallent派車系統，因為socket連線已經中斷");
                return new SocketStates() { };
            }
            var states = await SocketSendOut(requestJson, true, check_match_str);
            mhsLogger.LATToAGVS(requestJson);
            return states;
        }

        private async Task<SocketStates> SocketSendOut(string requestJson, bool waitReply, string check_match_str = null)
        {
            var states = await socketClient.Send(CreateSendOutBytes(requestJson), waitReply, check_match_str);
            return states;

        }
        private byte[] CreateSendOutBytes(string json)
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] endChar = new byte[2] { 0x2a, 0x0d };
            List<byte> sendOutBytes = new List<byte>();
            sendOutBytes.AddRange(data);
            sendOutBytes.AddRange(endChar);

            return sendOutBytes.ToArray();
        }
        public async Task<ONLINE_STATE> DownloadAgvcOnlineState(IAGVC agvc)
        {
            Dictionary<string, Dictionary<string, object>> headerdata = null;
            try
            {
                HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
                var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineModeQueryModelJson();
                var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson, "0102");
                if (states.ASCIIRev.Contains("*"))
                {
                    string jsonStr = states.ASCIIRev.Replace("*\r", "");
                    Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                    headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(revObj["Header"].ToString());
                    return headerdata["0102"]["Remote Mode"].ToString() == "1" ? ONLINE_STATE.ONLINE : ONLINE_STATE.OFFLINE;
                }
                else
                    return ONLINE_STATE.Unknown;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<ONLINE_STATE> DownloadAgvcOnlineState(string sid, string eq_name)
        {
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(sid, eq_name);
            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineModeQueryModelJson();
            var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson, "0102");
            if (states.ASCIIRev.Contains("*"))
            {
                string jsonStr = states.ASCIIRev.Replace("*\r", "");
                Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(revObj["Header"].ToString());
                return headerdata["0102"]["Remote Mode"].ToString() == "1" ? ONLINE_STATE.ONLINE : ONLINE_STATE.OFFLINE;
            }
            else
                return ONLINE_STATE.Unknown;

        }

        public async Task<ONLINE_STATE> AgvcOnOffLineRequst(string sid, string eq_name, ONLINE_STATE stateReq)
        {
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(sid, eq_name);

            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineRequestJson(stateReq == ONLINE_STATE.OFFLINE ? 0 : 1, -1);
            var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson, "0104");
            if (states.ASCIIRev.Contains("*"))
            {
                string jsonStr = states.ASCIIRev.Replace("*\r", "");
                Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(revObj["Header"].ToString());

                if (headerdata.ContainsKey("0104"))
                {
                    return headerdata["0104"]["Return Code"].ToString() == "0" ? stateReq : ONLINE_STATE.Unknown;
                }
                else
                {
                    return ONLINE_STATE.Unknown;
                }
            }
            else
                return ONLINE_STATE.Unknown;
        }
        public async Task<ONLINE_STATE?> AgvcOnOffLineRequst(IAGVC agvc, ONLINE_STATE stateReq, int currentStation)
        {
            ////先報一次0105
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
            //var stateReportObj = stateReport.CreateStateReportDataModel(agvc.agvcStates);
            //bool state_report_success = await RunningStatusReport(stateReportObj);

            //if (!state_report_success)
            //{
            //    logger.WarnLog($"要求{stateReq}失敗，因為上報0105未成功，與派車系統的通訊可能有問題(報文格式)");
            //    return ONLINE_STATE.Unknown;
            //}

            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineRequestJson(stateReq == ONLINE_STATE.OFFLINE ? 0 : 1, currentStation);

            var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson, "0104");
            if (states.ASCIIRev.Contains("*"))
            {
                string jsonStr = states.ASCIIRev.Replace("*\r", "");
                Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(revObj["Header"].ToString());

                if (headerdata.ContainsKey("0104"))
                {
                    return headerdata["0104"]["Return Code"].ToString() == "0" ? stateReq : agvc.agvcStates.States.EOnlineState;
                }
                else
                {
                    return ONLINE_STATE.Unknown;
                }
            }
            else
                return ONLINE_STATE.Unknown;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                socketClient.Disconnect();
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~KingAllantAPI()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
