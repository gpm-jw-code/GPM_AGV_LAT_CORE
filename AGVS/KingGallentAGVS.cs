using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Parameters;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public bool isReconnecting { get; set; } = false;
        /// <summary>
        ///  
        /// </summary>
        public TcpSocketClient tcpSocketClient { get; private set; }

        public List<IAGVC> RegistedAgvcList { get; set; } = new List<IAGVC>();
        public IAgvsHandShakeable agvsApi { get; set; }
        public KingAllantAPI _agvsApi => (KingAllantAPI)agvsApi;

        public List<IAgvcInfoToAgvs> BindingAGVCInfoList { get; set; } = null;
        public string VenderName { get; set; } = "晶捷能";
        public List<clsHostExecuting> ExecuteTaskList { get; set; } = new List<clsHostExecuting>();

        private ILogger logger;
        private MessageHandShakeLogger mhsLogger = new MessageHandShakeLogger();
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
            bool connected = ClientSideConnect(out err_msg);
            Task.Run(async () =>
            {
                if (SystemParams.IsAGVS_Simulation)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            var bodyData = AGVSManager.CurrentAGVS.BindingAGVCInfoList.ToDictionary(agvc_info => (agvc_info as AgvcInfoForKingAllant).SID, agvc_info => (agvc_info as AgvcInfoForKingAllant).EQName);
                            StringContent content = new StringContent(JsonConvert.SerializeObject(bodyData));
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                            var res = await client.PostAsync(SystemParams.KingGallentAGVSEmulatorServerUrl + $"/UpdateAGVCBind", content);
                            var status_code = res.StatusCode;
                            if (status_code == System.Net.HttpStatusCode.OK)
                                logger.InfoLog($"已同步AGVC資訊到KinGallent模擬派車系統");
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            });
            return connected;
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
                Console.WriteLine("{0}:{1}", agvsParameters.tcpParams.HostIP, agvsParameters.tcpParams.HostPort);
                tcpSocketClient = new TcpSocketClient(agvsParameters.tcpParams.HostIP, agvsParameters.tcpParams.HostPort);
                tcpSocketClient.OnMessageReceive += TcpSocketClient_OnMessageReceive;
                tcpSocketClient.OnDisconnect += TcpSocketClient_OnDisconnect;
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

        private void TcpSocketClient_OnDisconnect(object sender, EventArgs e)
        {

            connected = false;
            logger.WarnLog($"KingGallent 派車系統斷線");

            if (!isReconnecting)
            {

                Task.Run(async () =>
                {
                    isReconnecting = true;
                    while (!ClientSideConnect(out string message))
                    {
                        logger.WarnLog($"嘗試與派車系統重新連線失敗...");
                        await Task.Delay(1000);
                    }
                    isReconnecting = false;
                    logger.WarnLog($"嘗試與派車系統重新連線成功!");

                });
            }
        }

        private void TcpSocketClient_OnMessageReceive(object sender, SocketStates _SocketStates)
        {
            try
            {
                string[] splitedAry = _SocketStates.ASCIIRev.Replace("*", ";").Split(';');
                foreach (var jsonStr in splitedAry)
                {
                    if (!jsonStr.Contains("SID"))
                        continue;
                    mhsLogger.AGVSToLAT(jsonStr);
                    Dictionary<string, object> revObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                    var headerdata = JsonConvert.DeserializeObject<Dictionary<string, object>>(revObj["Header"].ToString());
                    OnHostMessageReceived?.Invoke(this, revObj);

                    if (headerdata.ContainsKey("0301") | headerdata.ContainsKey("0305"))
                    {
                        logger.WarnLog($"AGVS Task Down..{jsonStr}");
                        var taskExecutingState = new clsHostExcutingState(_SocketStates, revObj);
                        OnTaskDownloadRecieved?.Invoke(this, taskExecutingState);
                    }
                    else
                    {
                        //logger.InfoLog($"AGVS Acknowleage : {jsonStr}");
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
            return await _agvsApi.RunningStatusReport(reportObj);
        }


        public class clsHostExcutingState : IAGVSExecutingState
        {
            public TcpSocketClient socketClient { get; private set; }
            public dynamic executingObject { get; set; }
            public ORDER_STATE state { get; set; } = ORDER_STATE.WAIT_EXECUTE;

            public clsHostExcutingState(SocketStates socketState, Dictionary<string, object> executingObject)
            {
                this.socketClient = new TcpSocketClient();
                socketClient.socket = socketState.socket;
                socketClient.socketState = socketState;
                this.executingObject = executingObject;
            }
        }
    }


}
