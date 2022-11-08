using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.MapState;

namespace GPM_AGV_LAT_CORE.Emulators.KingGallentAGVS
{

    public partial class KingGallentAgvsEmulator
    {
        protected ILogger logger = new LoggerInstance(typeof(KingGallentAgvsEmulator));
        private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();
        public Dictionary<string, BindingAGVC> DictAGVC = new Dictionary<string, BindingAGVC>()
        {
            {"AGV_001", new BindingAGVC("AGV_001","001:001:001") },
            {"AGV_002", new BindingAGVC("AGV_002","002:001:002") },
            {"AGV_003", new BindingAGVC("AGV_003","003:001:003") },
        };

        public KingGallentAgvsEmulator(string ip, int port)
        {
            server = new TcpSocketServer(ip, port);
        }

        protected TcpSocketServer server { get; set; }

        virtual public void Start()
        {
            foreach (var agv in DictAGVC.Values)
            {
                agv.OnTaskDownload += Agv_OnTaskDownload;
            }

            Task.Factory.StartNew(() =>
            {
                server.OnMessageReceive += Server_OnClientMessageRev;
                server.OnClientConnected += Server_OnClientConnected;
                server.Listen();
            });
        }

        private void Agv_OnTaskDownload(object sender, TaskDownObject taskDownObject)
        {
            TaskDownload(taskDownObject.SID, taskDownObject.EQName, taskDownObject.taskName, taskDownObject.stationID);
        }

        virtual protected void Server_OnClientConnected(object sender, Socket socket)
        {
            logger.InfoLog($"AGVC Connected {socket.RemoteEndPoint.ToString()}");
        }

        virtual public void Server_OnClientMessageRev(object sender, SocketStates e)
        {
            Task.Run(() =>
            {
                try
                {

                    string[] jsonstrAry = e.ASCIIRev.Replace("*", ";").Split(';');

                    foreach (var jsonStr in jsonstrAry)
                    {
                        if (!jsonStr.Contains("SID"))
                            continue;
                        HandshakeResponseDataHelper helper = new HandshakeResponseDataHelper();
                        bool success = helper.CreateTemplate(jsonStr);
                        if (!success)
                            return;

                        Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                        var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(obj["Header"].ToString());

                        string SID = obj["SID"].ToString();
                        string EQName = obj["EQName"].ToString();

                        string agvKey = SID + EQName;
                        if (!clients.ContainsKey(agvKey))
                            clients.Add(agvKey, null);
                        clients[agvKey] = e.socket;

                        var headerCode = messageData.Keys.FirstOrDefault();

                        Dictionary<string, object> returnData = null;
                        if (headerCode == "0101")
                        {
                            logger.TraceLog($"AGVC Request(0101):{jsonStr}");
                            if (EQName == "AGV_002")
                                returnData = helper.Create0102MessageData(0);//模擬offline
                            else
                                returnData = helper.Create0102MessageData(1);

                        }
                        else if (headerCode == "0103")
                        {
                            //logger.TraceLog($"AGVC Request(0103):{json}");
                            returnData = helper.Create0104MessageData(0);
                        }
                        else if (headerCode == "0105")
                        {
                            //logger.TraceLog($"AGVC Request(0105):{json}");
                            returnData = helper.create0106MessageData(0);
                        }

                        else if (headerCode == "0303") // Task Feedback
                        {
                            var agvc = DictAGVC[EQName];
                            //logger.TraceLog($"AGVC Request(0303):{json}");
                            var agvReturnData = messageData[headerCode];
                            var obj22 = JsonConvert.DeserializeObject<Dictionary<string, object>>(agvReturnData.ToString());
                            string taskName = obj22["Task Name"].ToString();
                            int status = Convert.ToInt16(obj22["Task Status"].ToString());
                            if (taskName == agvc.ExecutingTaskName && status == 4)
                                agvc.SetWorkFlowResume();

                            returnData = helper.create0304MessaeData(0);
                        }
                        else if (headerCode == "0302") //0301 TaskDownload 的回覆
                        {
                            //logger.WarnLog($"AGVC Ack(0302):{json}");
                            return;
                        }
                        else if (headerCode == "0306") //0305 AGVS Reset Command  的回覆
                        {
                            //logger.TraceLog($"AGVC Ack(0306):{json}");
                            return;
                        }

                        if (returnData != null)
                        {
                            try
                            {
                                string ackJson = JsonConvert.SerializeObject(returnData);
                                List<byte> sendOutBytes = new List<byte>();
                                sendOutBytes.AddRange(Encoding.ASCII.GetBytes(ackJson));
                                sendOutBytes.AddRange(new byte[2] { 0x2a, 0x0d });
                                e.socket.Send(sendOutBytes.ToArray());
                                //logger.TraceLog($"AGVS Ack({headerCode}):{ackJson}");
                            }
                            catch (Exception ex)
                            {
                                logger.FatalLog($"AGVS Ack({headerCode}) Err", ex);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.ErrorLog($"Server_OnClientMessageRev Err", ex);
                }

            });

        }
        public async Task TaskDownload(string SID, string EQName, string taskName, string stationID)
        {
            await Task.Factory.StartNew(() =>
            {
                Socket client = FindColient(SID, EQName);
                if (client == null)
                {
                    return;
                }
                HandshakeRunningStatusReportHelper requestHelper = new HandshakeRunningStatusReportHelper(SID, EQName);
                List<byte> bytes = new List<byte>();
                bytes.AddRange(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(requestHelper.CreateTaskDownload(taskName, stationID))));
                bytes.AddRange(new byte[] { 0x2A, 0x0D });
                client.Send(bytes.ToArray());
            });
        }

        public async Task<OrderResult> CreateNewOrder(string SID, string EQName, OrderTask order, bool waitOtherTaskFinish = true)
        {
            bool agvcExist = DictAGVC.TryGetValue(EQName, out BindingAGVC agvc);
            if (!agvcExist)
                return new OrderResult(false, OrderResult.RUN_STATE.FAIL)
                {
                    ErrorMessage = $"{EQName} 是一台靈車??(不存在)"
                };

            var non_exist_station_ids = order.Stations.FindAll(stationID => agvc.mapInfo.station_id_list.Contains(stationID) == false);
            if (non_exist_station_ids.Count != 0)
            {

                return new OrderResult(false, OrderResult.RUN_STATE.FAIL) { ErrorMessage = $"部分要求的站點({string.Join(",", non_exist_station_ids)})不存在於當前地圖" };
            }

            return agvc.NewOrder(order, waitOtherTaskFinish);

        }



        /// <summary>
        /// 0305 當 AGVC 收到此 CMD 後，會依 Reset Mode 判斷是否為立即停下，停在點與點中間是可行的。若否，則待移動到 Node 上才停止。
        /// </summary>
        public void AGVSReset(string EQName, int ResetMode, string SID = null)
        {

            if (SID == null)
                SID = DictAGVC[EQName].SID;

            Socket client = FindColient(SID, EQName);
            if (client == null)
            {
                return;
            }
            HandshakeRunningStatusReportHelper requestHelper = new HandshakeRunningStatusReportHelper(SID, EQName);
            DictAGVC[EQName].CancelNavigating();
            Send(client, requestHelper.CreateAGVSResetExcute(ResetMode));
        }

        private Socket FindColient(string SID, string EQName)
        {
            if (clients.Count == 0)
                return null;

            string agvKey = SID + EQName;
            clients.TryGetValue(agvKey, out Socket client);
            if (client == null)
            {
                client = clients.First().Value;
            }
            return client;
        }
        private void Send(Socket client, Dictionary<string, object> data)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data)));
            bytes.AddRange(new byte[] { 0x2A, 0x0D });
            client.Send(bytes.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EQName"></param>
        /// <param name="mapInfo"></param>
        public void UpdateMapState(string EQName, MapInfo mapInfo)
        {
            if (DictAGVC.TryGetValue(EQName, out BindingAGVC agvc))
            {
                agvc.mapInfo = mapInfo;
            }
        }

        public void UpdateAGVCBind(Dictionary<string, string> sid_eqNames)
        {

            foreach (var agv in DictAGVC.Values)
            {
                agv.OnTaskDownload -= Agv_OnTaskDownload;
            }

            DictAGVC = new Dictionary<string, BindingAGVC>();
            foreach (var item in sid_eqNames)
            {
                DictAGVC.Add(item.Value, new BindingAGVC(item.Value, item.Key));

            }
            foreach (var agv in DictAGVC.Values)
            {
                agv.OnTaskDownload += Agv_OnTaskDownload;
            }
        }
    }
}
