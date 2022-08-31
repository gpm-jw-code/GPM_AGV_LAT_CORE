using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
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
                SendMessageOut(json);
            }
        }

        internal void RunningStatusReport(object agvcRunningStateData)
        {
            SendMessageOut(JsonConvert.SerializeObject(agvcRunningStateData));
        }

        private void SendMessageOut(string requestJson)
        {
            string json = requestJson + "*CR";
            try
            {
                socketClient.Send(Encoding.ASCII.GetBytes(json));
            }
            catch (Exception ex)
            {
                Console.WriteLine("發送封包給派車時發生錯誤{0}", ex.Message);
            }
        }
    }
}
