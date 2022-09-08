using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (connected)
                CheckOnlineStateFromAGVSRequest?.Invoke(this, this);
            agvcStates.States.EConnectionState = connected ? CONNECTION_STATE.CONNECTED : CONNECTION_STATE.DISCONNECT;
            return connected;
        }

        virtual protected bool ConnectoAGVInstance()
        {
            throw new NotImplementedException();
        }


        public async Task SyncState()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                try
                {
                    await SyncStateInstance();
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
            bool anyTaskExecute = orderList_LAT.Any(order => order.State == ORDER_STATE.EXECUTING);
            if (anyTaskExecute)
                agvcStates.States.ERunningState = RUNNING_STATE.RUNNING;
            else
                agvcStates.States.ERunningState = RUNNING_STATE.IDLE;

            return Task.CompletedTask;
        }

        virtual protected async Task SyncOrderStateInstance()
        {
            throw new NotImplementedException();
        }

        virtual protected async Task SyncStateInstance()
        {
            throw new NotImplementedException();
        }

        virtual public void AddHostOrder(clsHostExecuting order)
        {
            orderList_LAT.Add(order);
            //模擬
            Task.Run(async () =>
            {
                order.State = ORDER_STATE.EXECUTING;
                await Task.Delay(TimeSpan.FromSeconds(5));
                order.State = ORDER_STATE.COMPLETE;
            });
        }

        public void AGVCDataConvertToLATFormat(object agvcData)
        {
        }


    }
}
