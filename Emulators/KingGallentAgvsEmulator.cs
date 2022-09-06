using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.Emulators
{
    public class KingGallentAgvsEmulator
    {
        private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();

        public KingGallentAgvsEmulator(string ip, int port)
        {
            server = new TcpSocketServer(ip, port);
        }

        protected TcpSocketServer server { get; set; }
        public void Start(string ip, int port)
        {
            server = new TcpSocketServer(ip, port);
            server.OnClientMessageRev += Server_OnClientMessageRev;
            server.Listen();
        }

        public void Start()
        {
            server.OnClientMessageRev += Server_OnClientMessageRev;
            server.Listen();
        }

        virtual public void Server_OnClientMessageRev(object sender, SocketStates e)
        {
            HandshakeResponseDataHelper helper = new HandshakeResponseDataHelper(e.ASCIIRev);
            Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.ASCIIRev.Replace("*CR", ""));
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
                returnData = helper.Create0102MessageData(1);
            }
            else if (headerCode == "0103")
            {
                returnData = helper.Create0104MessageData(0);
            }
            else if (headerCode == "0105")
            {
                returnData = helper.create0106MessageData(0);
            }
            else if (headerCode == "0302") //0301 TaskDownload 的回覆
            {

            }
            else if (headerCode == "0303") // Task Feedback
            {
                returnData = helper.create0304MessaeData(0);

            }


            if (returnData != null)
            {
                try
                {
                    e.socket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(returnData) + "*CR"));
                }
                catch (Exception ex)
                {
                }
            }

        }
        public void TaskDownload(string SID, string EQName, string taskName)
        {
            if (clients.Count == 0)
                return;

            string agvKey = SID + EQName;
            clients.TryGetValue(agvKey, out Socket client);
            if (client == null)
            {
                client = clients.First().Value;
            }
            HandshakeRunningStatusReportHelper requestHelper = new HandshakeRunningStatusReportHelper(SID, EQName);
            client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(requestHelper.CreateTaskDownload(taskName)) + "*CR"));
        }
    }
}
