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
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public class KingAllantAPI : IAgvsHandShakeable
    {


        public readonly TcpSocketClient socketClient;

        public KingAllantAPI(TcpSocketClient socketClient)
        {
            this.socketClient = socketClient;
        }

        public void OnlineRequest(int[] agvNos)
        {
            foreach (var no in agvNos)
            {
                HandshakeRunningStatusReportHelper request = new HandshakeRunningStatusReportHelper($"00{no}:001:001", $"AGV_00{no}");
                var json = JsonConvert.SerializeObject(request.CreateOnlineOfflineRequest(1, no * 1000));
                ReportRequestMessageSendOut(json, out SocketStates states);
            }
        }

        public bool RunningStatusReport(Dictionary<string, object> agvcRunningStateData)
        {
            ReportRequestMessageSendOut(JsonConvert.SerializeObject(agvcRunningStateData), out SocketStates states);
            return states.receieveLen != 0;
        }


        public void TaskDownloadReport(IAGVC agvc, bool success, IAGVSExecutingState executingState)
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
            AckMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply));
        }
        public void ResetReport(IAGVC agvc, bool success, IAGVSExecutingState executingState)
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

        public void TaskStateFeedback(clsHostExecuting order)
        {
            var agvcInfo = order.ExecuteingAGVC.agvcInfoForagvs as AgvcInfoForKingAllant;
            Dictionary<string, object> taskDownloadReply = CreateModelBase(agvcInfo);
            taskDownloadReply["Header"] = new Dictionary<string, object>()
            {
                 {"0303", new Dictionary<string, object>()
                        {
                            { "Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                            { "Task Name", order.latOrderDetail.taskName },
                            { "Task Status", GetTaskStatusFromLATState(order.State)},
                        }
                    }
            };
            ReportRequestMessageSendOut(JsonConvert.SerializeObject(taskDownloadReply), out SocketStates states);
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

        private bool AckMessageSendOut(string json)
        {
            json = json + "*CR";
            try
            {
                SocketStates ret = socketClient.Send(Encoding.ASCII.GetBytes(json), false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("發送封包給派車時發生錯誤{0}", ex.Message);
                return false;
            }
        }

        private bool ReportRequestMessageSendOut(string requestJson, out SocketStates states)
        {
            string json = requestJson + "*CR";
            states = socketClient.Send(Encoding.ASCII.GetBytes(json), true);
            return states.receieveLen > 0;
        }

        public ONLINE_STATE DownloadAgvcOnlineState(IAGVC agvc)
        {
            //先報一次0105
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
            var stateReportObj = stateReport.CreateStateReportDataModel(agvc.agvcStates);

            RunningStatusReport(stateReportObj);
            var onlineModeQueryModelJson = stateReport.CreateOnlineOfflineModeQueryModelJson();
            ReportRequestMessageSendOut(onlineModeQueryModelJson, out SocketStates states);
            if (states.ASCIIRev.Contains("*CR"))
            {
                string jsonStr = states.ASCIIRev.Replace("*CR", "");
                Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,object>>>(revObj["Header"].ToString());
                return headerdata["0102"]["Remote Mode"].ToString() == "1" ? ONLINE_STATE.ONLINE : ONLINE_STATE.OFFLINE;
            }
            else
                return ONLINE_STATE.Unknown;

        }
    }
}
