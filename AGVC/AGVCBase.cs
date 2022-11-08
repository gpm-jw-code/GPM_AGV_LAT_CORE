using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.MapState;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class AGVCBase : IAGVC
    {
        public AGVCBase()
        {
        }


        public string ID { get; set; }
        public int Index { get; set; }

        virtual public string EQName
        {
            get
            {
                if (agvsBinding.agvsType == AGVS_TYPES.KINGGALLENT)
                {
                    return (agvcInfos as AgvcInfoForKingAllant).EQName;
                }
                else
                {
                    return $"{agvcType}-{ID}";
                }
            }
        }

        public AGVC_TYPES agvcType { get; set; } = AGVC_TYPES.Unkown;

        public List<clsHostExecuting> orderList_LAT { get; set; } = new List<clsHostExecuting>();
        public AGVCParameters agvcParameters { get; set; } = new AGVCParameters();
        public AGVCStateStore agvcStates { get; set; } = new AGVCStateStore();
        public IAgvcInfoToAgvs agvcInfos { get; set; }
        private IAGVS _agvsBinding { get; set; }
        public IAGVS agvsBinding
        {
            get => _agvsBinding; set
            {
                _agvsBinding = value;
            }
        }

        public ILogger logger { get; set; }

        public event EventHandler OrderStateOnChnaged;
        public event EventHandler<AGVCStateStore> StateOnChanged;
        public event EventHandler<IAGVC> CheckOnlineStateFromAGVSRequest;
        public event EventHandler<OnOffLineRequest> OnlineOfflineRequest;

        /// <summary>
        /// 狀態變化事件委派
        /// </summary>
        virtual protected void StateChangedDelagate()
        {
            StateOnChanged?.Invoke(this, agvcStates);
        }


        public bool ConnectToAGV()
        {
            agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTING;
            bool connected = ConnectoAGVInstance();
            agvcStates.States.EConnectionState = connected ? CONNECTION_STATE.CONNECTED : CONNECTION_STATE.DISCONNECT;


            return connected;
        }

        virtual public async Task OnlineStateInitProcess()
        {
            logger.InfoLog($"{EQName} 上線狀態初始化開始");
            int ind = 1;
            var state = ONLINE_STATE.Unknown;
            while (state == ONLINE_STATE.Unknown | state == ONLINE_STATE.Downloading)
            {
                agvcStates.States.EOnlineState = ONLINE_STATE.Downloading;
                logger.InfoLog($"{EQName} 嘗試下載上線狀態...({ind})");
                await Task.Delay(1000);
                CheckOnlineStateFromAGVSRequest?.Invoke(this, this);
                state = this.agvcStates.States.EOnlineState;
                ind += 1;
            }

            ind = 1;
            while (agvcStates.States.EOnlineState != ONLINE_STATE.ONLINE)
            {
                logger.InfoLog($"{EQName} 上線請求狀態...({ind})");
                OnlineOfflineRequest?.Invoke(this, new OnOffLineRequest(this, ONLINE_STATE.ONLINE));
                ind += 1;
            }
        }

        virtual public List<string> GetMapNames()
        {
            throw new NotImplementedException();
        }

        virtual protected Task<MapInfo> LoadMapStationStored()
        {
            throw new NotImplementedException();
        }

        virtual protected bool ConnectoAGVInstance()
        {
            throw new NotImplementedException();
        }


        public async Task SyncState()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                try
                {
                    await SyncStateInstance();

                    if (agvcStates.States.EOnlineState != ONLINE_STATE.Unknown)
                        StateChangedDelagate();
                    //logger.InfoLog("車體狀態完成同步");
                }
                catch (Exception ex)
                {
                    //logger.ErrorLog(ex);
                }
            }
        }
        public async Task SyncOrdersState()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                try
                {
                    await SyncOrderStateInstance();
                    //logger.InfoLog("車體狀態完成同步");
                }
                catch (Exception ex)
                {
                    //logger.ErrorLog(ex);
                }
            }
        }

        public async Task SyncSyncOrderExecuteState()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                try
                {
                    await SyncSyncOrderExecuteStateInstance();
                    //logger.InfoLog("訂單執行狀態完成同步");
                }
                catch (Exception ex)
                {
                    logger.WarnLog(ex.Message);
                }
            }
        }

        virtual protected Task SyncSyncOrderExecuteStateInstance()
        {

            var excutingOrder = orderList_LAT.FirstOrDefault(order => order.State == ORDER_STATE.EXECUTING);
            bool anyTaskExecute = !orderList_LAT.All(order => order.State == ORDER_STATE.COMPLETE | order.State == ORDER_STATE.CANCELED);

            agvcStates.MapStates.navigationState.IsNavigating = anyTaskExecute;

            if (anyTaskExecute)
            {
                agvcStates.States.ERunningState = RUNNING_STATE.RUNNING;
                agvcStates.MapStates.navigationState.targetStationID = excutingOrder.latOrderDetail.action.stationID;
                agvcStates.MapStates.navigationState.pathStations = excutingOrder.latOrderDetail.action.paths;

                var nextOrder = orderList_LAT.FirstOrDefault(order => order.State == ORDER_STATE.WAIT_EXECUTE);
                if (nextOrder != null)
                    agvcStates.MapStates.navigationState.nextStationID = nextOrder.latOrderDetail.action.stationID;
                else
                    agvcStates.MapStates.navigationState.nextStationID = "";

            }
            else
            {
                agvcStates.MapStates.navigationState.targetStationID = agvcStates.MapStates.navigationState.nextStationID = "";
                agvcStates.States.ERunningState = RUNNING_STATE.IDLE;
            }

            return Task.CompletedTask;
        }

        virtual protected async Task SyncOrderStateInstance()
        {
            throw new NotImplementedException();
        }

        virtual protected async Task SyncStateInstance()
        {
            if (SystemParams.IsAGVS_Simulation)
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {

                        StringContent content = new StringContent(JsonConvert.SerializeObject(agvcStates.MapStates.currentMapInfo));
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        var res = await client.PostAsync(SystemParams.KingGallentAGVSEmulatorServerUrl + $"/UpdateMapState?EqName={EQName}", content);
                        var status_code = res.StatusCode;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        virtual public void AddHostOrder(clsHostExecuting order)
        {
            orderList_LAT.Add(order);
        }

        public void AGVCDataConvertToLATFormat(object agvcData)
        {
        }

        virtual public Task<ORDER_STATE> TaskStateDownload(string taskName)
        {
            throw new NotImplementedException();
        }

        virtual public Task<object> GetNativeAlarmState()
        {
            throw new NotImplementedException();
        }

        virtual public AlarmStates GetLatAlarm(object nativeAlarm)
        {
            throw new NotImplementedException();
        }

        virtual public Task PauseNavigate()
        {
            throw new NotImplementedException();
        }

        virtual public Task ResumeNavigate()
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> RelocProcess()
        {
            throw new NotImplementedException();
        }

        public class OnOffLineRequest
        {
            public IAGVC agvc { get; private set; }
            public ONLINE_STATE stateReq { get; private set; }

            public OnOffLineRequest(IAGVC agvc, ONLINE_STATE stateReq)
            {
                this.agvc = agvc;
                this.stateReq = stateReq;
            }
        }

    }
}
