using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
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
        /// <summary>
        ///  
        /// </summary>
        public TcpSocketClient tcpSocketClient { get; private set; }

        public List<IAGVC> RegistedAgvcList { get; set; } = new List<IAGVC>();
        public IAgvsHandShakeable agvsApi { get; set; }
        public KingAllantAPI _agvsApi => (KingAllantAPI)agvsApi;

        public List<IAgvcInfoToAgvs> BindingAGVCInfoList { get; set; } = null;
        public string VenderName { get; set; } = "晶捷能";
        private ILogger logger;
        public KingGallentAGVS()
        {
            logger = new LoggerInstance(GetType());
        }

        public event EventHandler<object> OnHostMessageReceived;
        public event EventHandler<IAGVSExecutingState> OnTaskDownloadRecieved;

        /// <summary>
        /// 把自己當成一部車連接到晶捷能派車平台
        /// </summary>
        /// <returns></returns>
        public bool ConnectToHost(out string err_msg)
        {
            return ClientSideConnect(out err_msg);
        }

        /// <summary>
        /// AGVC->AGVS Socket Interface
        /// </summary>
        /// <param name="err_msg"></param>
        /// <returns></returns>
        private bool ClientSideConnect(out string err_msg)
        {
            try
            {
                tcpSocketClient = new TcpSocketClient(agvsParameters.tcpParams.HostIP, agvsParameters.tcpParams.HostPort);
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
                string[] splitedAry = _SocketStates.ASCIIRev.Replace("*CR", ";").Split(';');
                foreach (var jsonStr in splitedAry)
                {
                    if (jsonStr == "" | jsonStr == null)
                        continue;
                    Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                    var headerdata = JsonConvert.DeserializeObject<Dictionary<string, object>>(revObj["Header"].ToString());
                    OnHostMessageReceived?.Invoke(this, revObj);

                    if (headerdata.ContainsKey("0301") | headerdata.ContainsKey("0305"))
                    {
                        logger.WarnLog($"AGVS Task Down..{jsonStr}");
                        OnTaskDownloadRecieved?.Invoke(this, new clsHostExcutingState(_SocketStates, revObj));
                    }
                    else
                    {
                        logger.InfoLog($"AGVS Acknowleage : {jsonStr}");
                    }

                }


            }
            catch (Exception ex)
            {
                logger.FatalLog($"TcpSocketClient_OnMessageReceive:{ex.Message}", ex);
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


        public class clsHostExcutingState : IAGVSExecutingState
        {
            public TcpSocketClient socketClient { get; private set; }
            public dynamic executingObject { get; set; }
            public clsHostExcutingState(SocketStates socketState, Dictionary<string, object> executingObject)
            {
                this.socketClient = new TcpSocketClient();
                this.socketClient.tcpClient = new System.Net.Sockets.TcpClient();
                socketClient.tcpClient.Client = socketState.socket;
                socketClient.socketState = socketState;

                this.executingObject = executingObject;

            }
        }
    }


}
