using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public class KingAllantAPI : IAgvsApi
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
                SendMessageOut(json, true);
            }
        }

        public bool RunningStatusReport(Dictionary<string, object> agvcRunningStateData)
        {
            return SendMessageOut(JsonConvert.SerializeObject(agvcRunningStateData), true);
        }

        public void TaskDownloadReport(string SID, string EQName, int returnCode)
        {
            Dictionary<string, object> taskDownloadReply = new Dictionary<string, object>()
            {
                { "SID",SID},
                { "EQName",EQName},
                { "System Bytes",302},
                { "Header",new Dictionary<string, object>()
                {
                    {"0302", new Dictionary<string, object>()
                        {
                            { "Return Code",returnCode }
                        }
                    }
                } }
            };
            SendMessageOut(JsonConvert.SerializeObject(taskDownloadReply), false);
        }

        public void TaskStateFeedback(clsHostOrder order)
        {
            var agvcInfo = order.ExecuteingAGVC.agvcInfoForagvs as AgvcInfoForKingAllant;
            Dictionary<string, object> taskDownloadReply = new Dictionary<string, object>()
            {
                { "SID",agvcInfo.SID},
                { "EQName",agvcInfo.EQName},
                { "System Bytes",303},
                { "Header",new Dictionary<string, object>()
                {
                    {"0303", new Dictionary<string, object>()
                        {
                            { "Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                            { "Task Name", order.latOrderDetail.taskName },
                            { "Task Status", GetTaskStatusFromLATState(order.State)},
                        }
                    }
                } }
            };
            SendMessageOut(JsonConvert.SerializeObject(taskDownloadReply), true, out SocketStates states);
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

        private bool SendMessageOut(string requestJson, bool waitReply)
        {
            string json = requestJson + "*CR";
            try
            {
                SocketStates ret = socketClient.Send(Encoding.ASCII.GetBytes(json), waitReply);
                return waitReply ? ret.receieveLen > 0 : true;//TODO 
            }
            catch (Exception ex)
            {
                Console.WriteLine("發送封包給派車時發生錯誤{0}", ex.Message);
                return false;
            }
        }

        private bool SendMessageOut(string requestJson, bool waitReply, out SocketStates states)
        {
            string json = requestJson + "*CR";
            states = socketClient.Send(Encoding.ASCII.GetBytes(json), waitReply);
            return states.receieveLen > 0;
        }
    }
}
