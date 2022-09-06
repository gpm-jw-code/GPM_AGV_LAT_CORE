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
            logger = new LoggerInstance(GetType());
        }

        private ILogger logger;

        public string ID { get; set; }
        public int Index { get; set; }

        public string EQName => $"{agvcType}-{ID}";

        public AGVC_TYPES agvcType { get; set; } = AGVC_TYPES.Unkown;

        public List<clsHostOrder> orderList_LAT { get; set; } = new List<clsHostOrder>();
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

        public event EventHandler OrderStateOnChnaged;
        public event EventHandler<AGVCStateStore> StateOnChanged;

        protected void StateChangedDelagate()
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
                    WriteLog(" 車體狀態完成同步");
                }
                catch (Exception ex)
                {
                    WriteErrorLog("SyncState", ex.Message, ConsoleColor.Red);
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
                    WriteLog(" 訂單狀態完成同步");
                }
                catch (Exception ex)
                {
                    WriteErrorLog("SyncOrdersState", ex.Message, ConsoleColor.Red);
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
                    WriteLog(" 訂單執行狀態完成同步");
                }
                catch (Exception ex)
                {
                    WriteErrorLog("SyncSyncOrderExecuteState", ex.Message, ConsoleColor.Red);
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

        virtual public void AddHostOrder(clsHostOrder order)
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

        /// <summary>
        /// 寫LOG
        /// </summary>
        /// <param name="message"></param>
        protected void WriteLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{DateTime.Now} |AGVC-{GetType().Name}({EQName})| {message}");
        }
        /// <summary>
        /// 寫LOG
        /// </summary>
        /// <param name="message"></param>
        protected void WriteErrorLog(string method, string message, ConsoleColor color = ConsoleColor.DarkYellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now} |AGVC-{GetType().Name}|{method}| {message}");
        }
    }
}
