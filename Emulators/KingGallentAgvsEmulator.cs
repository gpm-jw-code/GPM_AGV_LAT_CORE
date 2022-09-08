using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.Emulators
{

    public class KingGallentAgvsEmulator
    {
        ILogger logger = new LoggerInstance(typeof(KingGallentAgvsEmulator));
        private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();

        public KingGallentAgvsEmulator(string ip, int port)
        {
            server = new TcpSocketServer(ip, port);
        }

        protected TcpSocketServer server { get; set; }

        virtual public void Start()
        {
            server.OnMessageReceive += Server_OnClientMessageRev;
            server.OnClientConnected += Server_OnClientConnected;
            server.Listen();
        }


        protected void Server_OnClientConnected(object sender, Socket socket)
        {
            //logger.InfoLog($"AGVC Connected {socket.RemoteEndPoint.ToString()}");
        }

        virtual public void Server_OnClientMessageRev(object sender, SocketStates e)
        {
            try
            {

                HandshakeResponseDataHelper helper = new HandshakeResponseDataHelper(e.ASCIIRev);
                string json = e.ASCIIRev.Replace("*CR", "");
                Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
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
                    //logger.TraceLog($"AGVC Request(0303):{json}");
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
                        //logger.FatalLog($"AGVS Ack({headerCode}) Err", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.ErrorLog($"Server_OnClientMessageRev Err", ex);
            }

        }
        public void TaskDownload(string SID, string EQName, string taskName)
        {
            Socket client = FindColient(SID, EQName);
            if (client == null)
            {
                return;
            }
            HandshakeRunningStatusReportHelper requestHelper = new HandshakeRunningStatusReportHelper(SID, EQName);
            client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(requestHelper.CreateTaskDownload(taskName)) + "*CR"));
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
    }
}
