using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Parameters;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    /// <summary>
    /// 晶捷能派車系統 
    /// </summary>
    public class KingGallentAGVS : IAGVS
    {
        public AGVS_TYPES agvsType { get; set; } = AGVS_TYPES.KINGGALLENT;
        public AGVSParameters agvsParameters { get; set; } = new AGVSParameters();
        public bool connected { get; set; }
        public TcpSocketClient tcpSocketClient { get; private set; }
        public List<IAGVC> RegistedAgvcList { get; set; } = new List<IAGVC>();
        public IAgvsApi agvsApi { get; set; }
        public KingAllantAPI _agvsApi => (KingAllantAPI)agvsApi;

        public List<IAgvcInfoToAgvs> BindingAGVCInfoList { get; set; } = null;
        public string VenderName { get; set; } = "晶捷能";

        public KingGallentAGVS()
        {
        }

        public event EventHandler<object> OnHostMessageReceived;
        public event EventHandler<object> OnTaskDownloadRecieved;

        /// <summary>
        /// 把自己當成一部車連接到晶捷能派車平台
        /// </summary>
        /// <returns></returns>
        public bool ConnectToHost(out string err_msg)
        {
            err_msg = "";
            try
            {
                //TODO實作 晶捷能 => TCP Socket
                tcpSocketClient = new TcpSocketClient(agvsParameters.tcpParams.HostIP, agvsParameters.tcpParams.HostPort, new KingAllantSocketStates());
                tcpSocketClient.OnMessageReceive += TcpSocketClient_OnMessageReceive;
                connected = tcpSocketClient.Connect(out err_msg);
                agvsApi = new KingAllantAPI(tcpSocketClient);
                return connected;
            }
            catch (Exception ex)
            {
                err_msg = ex.Message;
                return false;
            }
        }

        private void TcpSocketClient_OnMessageReceive(object sender, SocketStates _SocketStates)
        {
            try
            {
                KingAllantSocketStates state = (KingAllantSocketStates)_SocketStates;
                var revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(state.JSONCmd);
                OnHostMessageReceived?.Invoke(this, revObj);
                var headerdata = JsonConvert.DeserializeObject<Dictionary<string, object>>(revObj["Header"].ToString());

                if (headerdata.ContainsKey("0301") | headerdata.ContainsKey("0305"))
                    OnTaskDownloadRecieved?.Invoke(this, revObj);
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<bool> ReportAGVCState(IAGVC agvc, AGVCStateStore agvcState)
        {
            HandshakeRunningStatusReportHelper stateReport = new HandshakeRunningStatusReportHelper(agvc.agvcInfos as AgvcInfoForKingAllant);
            var reportObj = stateReport.CreateStateReportDataModel(new RunningStateReportModel
            {
                AGVStatus = (int)agvcState.States.ERunningState
            });
            return _agvsApi.RunningStatusReport(reportObj);
        }
    }
}
