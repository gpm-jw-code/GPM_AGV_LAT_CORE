using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Parameters;
using System;
using GangHaoAGV.AGV;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using System.Collections.Generic;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class GangHaoAGVC : IAGVC
    {
        public string ID { get; set; } = "0001";
        public AGVC_TYPES agvcType { get; set; } = AGVC_TYPES.GangHau;
        public AGVCParameters agvcParameters { get; set; } = new AGVCParameters();
        public AGVCStateStore agvcStates { get; set; } = new AGVCStateStore();
        public cAGV AGVInterface { get; set; }
        public int Index { get; set; } = 0;
        public string EQName => string.Join("_", new object[] { agvcType.ToString(), Index.ToString("X3") });

        public IAgvcInfoToAgvs agvcInfos { get; set; }
        public List<clsHostOrder> orderList_LAT { get; set; } = new List<clsHostOrder>();

        public event EventHandler StateOnChanged;

        public bool ConnectToAGV()
        {
            agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTING;
            AGVInterface = new cAGV(agvcParameters.tcpParams.HostIP);
            SyncState();
            return true;
        }


        public async void SyncState()
        {
            await Task.Delay(1);
            while (!AGVInterface.StatesPortConnected | AGVInterface.STATES == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine("等待罡豪State Port 連線...");
            }
            Console.WriteLine("罡豪State Port 已連線,開始同步車子狀態");
            agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTED;

            _ = Task.Run(async () =>
            {

                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    LocationStateSync();
                    BetteryStateSync();
                    agvcStates.States.EConnectionState = AGVInterface.StatesPortConnected ? CONNECTION_STATE.CONNECTED : CONNECTION_STATE.CONNECTING;
                    StateOnChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public bool TryExecuteOrder(clsHostOrder order, out string message)
        {
            message = null;

            //TODO 與車子確認是否可以執行任務

            return true;
        }

        /// <summary>
        /// 更新電池狀態
        /// </summary>
        virtual protected void BetteryStateSync()
        {
            agvcStates.BetteryState.remaining = AGVInterface.STATES.betteryState.remainPercent;
        }

        private void LocationStateSync()
        {
            var locationInfo = AGVInterface.STATES.locationInfo;
            agvcStates.MapStates.globalCoordinate.x = locationInfo.x;
            agvcStates.MapStates.globalCoordinate.y = locationInfo.y;
            agvcStates.MapStates.globalCoordinate.r = locationInfo.r;
        }

        void IAGVC.SyncOrdersState()
        {
            try
            {

            }
            catch (NotImplementedException)
            {
                Console.WriteLine("還沒實作 SyncOrdersState");
            }
        }

        void IAGVC.SyncSyncOrderExecuteState()
        {
            try
            {

            }
            catch (NotImplementedException)
            {
                Console.WriteLine("還沒實作 SyncSyncOrderExecuteState");
            }
        }
    }
}
