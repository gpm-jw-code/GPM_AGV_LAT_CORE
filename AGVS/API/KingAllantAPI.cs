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
    public class KingAllantAPI : IAgvsHandShakeable
    {


        public readonly TcpSocketClient socketClient;

        public MessageHandShakeLogger mhsLogger { get; } = new MessageHandShakeLogger();

        public KingAllantAPI(TcpSocketClient socketClient)
        {
            this.socketClient = socketClient;
        }


        public async Task<bool> RunningStatusReport(Dictionary<string, object> agvcRunningStateData)
        {
            SocketStates states = await ReportRequestMessageSendOut(JsonConvert.SerializeObject(agvcRunningStateData));
            return states.receieveLen != 0;
        }


        public async Task ReportTaskDownloadResult(IAGVC agvc, bool success, IAGVSExecutingState executingState = null)
        {
            AgvcInfoForKingAllant agvcInfo = (AgvcInfoForKingAllant)agvc.agvcInfos;
            Dictionary<string, object> taskDownloadReply = CreateModelBase(agvc);
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
                            { "Task Simplex", order.latOrderDetail.taskName },  //任務TC拆解段落下給AGVC(??? 工蝦小)
                            { "Task Sequence", order.latOrderDetail.taskName }, //該路徑的第幾個點位
                            { "Task Status", GetTaskStatusFromLATState(order.State)},
                        }
                    }
            };
            ReportRequestMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply));
        }

        private Dictionary<string, object> CreateModelBase(AgvcInfoForKingAllant agvcInfo)
        {
            Dictionary<string, object> taskDownloadReply = new Dictionary<string, object>()
            {
                { "SID",agvcInfo.SID},
                { "EQName",agvcInfo.EQName},
                { "System Bytes",302},
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
                    return 4;
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
            json = json + "*CR";
            mhsLogger.LATToAGVS(json);
            try
            {
                SocketStates ret = await socketClient.Send(Encoding.ASCII.GetBytes(json), false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("發送封包給派車時發生錯誤{0}", ex.Message);
                return false;
            }
        }

        private async Task<SocketStates> ReportRequestMessageSendOut(string requestJson)
        {
            string json = requestJson + "*CR";
            var states = await socketClient.Send(Encoding.ASCII.GetBytes(json), true);
            mhsLogger.LATToAGVS(json);
            return states;
        }

        public async Task<ONLINE_STATE> DownloadAgvcOnlineState(IAGVC agvc)
        {
            //先報一次0105
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
            var stateReportObj = stateReport.CreateStateReportDataModel(agvc.agvcStates);

            await RunningStatusReport(stateReportObj);
            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineModeQueryModelJson();
            var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson);
            if (states.ASCIIRev.Contains("*CR"))
            {
                string jsonStr = states.ASCIIRev.Replace("*CR", "");
                Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(revObj["Header"].ToString());
                return headerdata["0102"]["Remote Mode"].ToString() == "1" ? ONLINE_STATE.ONLINE : ONLINE_STATE.OFFLINE;
            }
            else
                return ONLINE_STATE.Unknown;

        }

        public async Task<ONLINE_STATE> AgvcOnOffLineRequst(IAGVC agvc, ONLINE_STATE stateReq)
        {
            //先報一次0105
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
            var stateReportObj = stateReport.CreateStateReportDataModel(agvc.agvcStates);
            await RunningStatusReport(stateReportObj);


            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineRequestJson(stateReq == ONLINE_STATE.OFFLINE ? 0 : 1, -1);
            var states = await ReportRequestMessageSendOut(onlineModeQueryModelJson);
            if (states.ASCIIRev.Contains("*CR"))
            {
                string jsonStr = states.ASCIIRev.Replace("*CR", "");
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
    }
}
