using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.Emulators
{

    public class KingGallentAgvsEmulator
    {
        protected ILogger logger = new LoggerInstance(typeof(KingGallentAgvsEmulator));
        private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();
        public Dictionary<string, AGVCSTate> DictAGVC = new Dictionary<string, AGVCSTate>()
        {
            {"AGV_001", new AGVCSTate("AGV_001","001:001:001") },
            {"AGV_002", new AGVCSTate("AGV_002","002:001:002") },
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

                    string[] jsonstrAry = e.ASCIIRev.Replace("*CR", ";").Split(';');

                    foreach (var jsonStr in jsonstrAry)
                    {
                        if (jsonStr == "")
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
                            //logger.TraceLog($"AGVC Request(0101):{json}");
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
                                e.socket.Send(Encoding.ASCII.GetBytes(ackJson + "*CR"));
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
                client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(requestHelper.CreateTaskDownload(taskName, stationID)) + "*CR"));
            });
        }

        public class OrderResult
        {
            public enum RUN_STATE
            {
                EXECUTING,
                WAITING,
                CANCELED,
                FAIL
            }
            public OrderResult(bool Success, RUN_STATE State)
            {
                this.Success = Success;
                this.State = State;
            }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public RUN_STATE State { get; set; }
        }


        public class TaskDownObject
        {
            public string SID { get; set; }
            public string EQName { get; set; }
            public string taskName { get; set; }
            public string stationID { get; set; }
        }

        public class AGVCSTate
        {

            public event EventHandler<TaskDownObject> OnTaskDownload;

            public AGVCSTate(string EQName, string SID)
            {
                this.EQName = EQName;
                this.SID = SID;
            }

            public string SID { get; set; }
            public string EQName { get; set; }
            public OrderTask ExecutingOrder { get; private set; } = null;
            public string ExecutingTaskName { get; private set; } = null;
            public Queue<OrderTask> waintingOrderLinks { get; private set; } = new Queue<OrderTask>();

            private ManualResetEvent OrderTaskResetEvent = new ManualResetEvent(false);
            private CancellationTokenSource NavigatingCancelTokenSource = new CancellationTokenSource();

            internal OrderResult NewOrder(OrderTask order, bool waitOtherTaskFinish)
            {
                if (ExecutingOrder != null && waitOtherTaskFinish) //當有任務鍊在進行中 但接受等待
                {
                    waintingOrderLinks.Enqueue(order);
                    return new OrderResult(true, OrderResult.RUN_STATE.WAITING);
                }
                OrderTaskResetEvent = new ManualResetEvent(true);
                _ = OrderLinkRun(order);
                return new OrderResult(true, OrderResult.RUN_STATE.EXECUTING);
            }


            private async Task OrderLinkRun(OrderTask order)
            {
                NavigatingCancelTokenSource = new CancellationTokenSource();
                ExecutingOrder = order;

                await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        while (order.StationsQueue.Count != 0)
                        {
                            OrderTaskResetEvent.Reset();//封鎖

                            OrderTask.StationInfo station = order.StationsQueue.Dequeue();
                            ExecutingTaskName = order.TaskID + $"station-{station.stationID}";
                            OnTaskDownload?.Invoke(this, new TaskDownObject { SID = this.SID, EQName = this.EQName, taskName = ExecutingTaskName, stationID = station.stationID });
                            OrderTaskResetEvent.WaitOne();

                        }

                        ExecutingOrder = null;
                        ExecutingTaskName = null;

                        if (waintingOrderLinks.Count != 0)
                        {
                            OrderTask nextExecuting = waintingOrderLinks.Dequeue();
                            Task.Factory.StartNew(() => OrderLinkRun(nextExecuting));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                }, NavigatingCancelTokenSource.Token);

            }

            internal void SetWorkFlowResume()
            {
                OrderTaskResetEvent.Set();
            }

            internal void CancelNavigating()
            {
                waintingOrderLinks.Clear();
                ExecutingOrder?.StationsQueue.Clear();
                NavigatingCancelTokenSource.Cancel();
            }
        }

        public async Task<OrderResult> CreateNewOrder(string SID, string EQName, OrderTask order, bool waitOtherTaskFinish = true)
        {
            bool agvcExist = DictAGVC.TryGetValue(EQName, out AGVCSTate agvc);
            if (!agvcExist)
                return new OrderResult(false, OrderResult.RUN_STATE.FAIL)
                {
                    ErrorMessage = $"{EQName} 是一台靈車??(不存在)"
                };

            return agvc.NewOrder(order, waitOtherTaskFinish);

        }



        /// <summary>
        /// 0305 當 AGVC 收到此 CMD 後，會依 Reset Mode 判斷是否為立即停下，停在點與點中間是可行的。若否，則待移動到 Node 上才停止。
        /// </summary>
        public void AGVSReset(string SID, string EQName, int ResetMode)
        {
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
            client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data) + "*CR"));
        }

        public class OrderTask
        {
            public OrderTask()
            {

            }
            public OrderTask(string TaskID, List<string> stationIDs)
            {
                this.TaskID = TaskID;
                Stations = stationIDs;
                foreach (var stationID in stationIDs)
                {
                    var stationInfo = new StationInfo() { stationID = stationID, Status = 0 };
                    StationsQueue.Enqueue(stationInfo);
                }
            }

            public string TaskID { get; set; }

            internal Queue<StationInfo> StationsQueue { get; set; } = new Queue<StationInfo>();
            public List<string> Stations { get; set; } = new List<string>();

            public List<string> ReachedStationIDList { get; private set; } = new List<string>();

            public void StationReachReport(string stationID)
            {
                ReachedStationIDList.Add(stationID);
            }

            public class StationInfo
            {
                public string stationID { get; set; }
                internal int Status { get; set; }
            }
        }


    }
}
