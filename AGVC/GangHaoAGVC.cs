using GPM_AGV_LAT_CORE.LATSystem;
using System;
using GangHaoAGV.AGV;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System.Linq;
using System.Collections.Generic;
using GangHaoAGV.API;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class GangHaoAGVC : AGVCBase
    {

        public ServerAPI RDSCoreServer = new ServerAPI() { baseUrl = SystemParams.GangHaoRDSCoreServerUrl };

        public GangHaoAGVC()
        {
            agvcType = AGVC_TYPES.GangHau;
        }

        public cAGV AGVInterface { get; set; }

        public new string EQName => string.Join("_", new object[] { agvcType.ToString(), Index.ToString("X3") });

        protected override bool ConnectoAGVInstance()
        {
            AGVInterface = new cAGV(agvcParameters.tcpParams.HostIP);
            return true;
        }

        protected override async Task SyncStateInstance()
        {
            while (!AGVInterface.StatesPortConnected | AGVInterface.STATES == null)
            {
                agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTING;
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine("等待罡豪State Port 連線...");
            }

            agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTED;
            agvcStates.BetteryState.remaining = AGVInterface.STATES.betteryState.remainPercent;
            agvcStates.MapStates.globalCoordinate.x = AGVInterface.STATES.locationInfo.x;
            agvcStates.MapStates.globalCoordinate.y = AGVInterface.STATES.locationInfo.y;
            agvcStates.MapStates.globalCoordinate.r = AGVInterface.STATES.locationInfo.r;

        }

        protected override Task SyncOrderStateInstance()
        {
            return base.SyncOrderStateInstance();
        }



        protected override async Task SyncSyncOrderExecuteStateInstance()
        {
            await Task.Run(async () =>
            {
                List<string> orderIDList = orderList_LAT.Select(order => order.latOrderDetail.taskName).ToList();
                foreach (var order in orderList_LAT)
                {
                    var orderID = order.latOrderDetail.taskName;
                    GangHaoAGV.Models.Order.OrderDetails orderState = await RDSCoreServer.QueryOrderState(orderID);
                    order.State = GetLatOrderStateByGangOrderState(orderState.state);
                }
            });
            await base.SyncSyncOrderExecuteStateInstance();
        }

        private ORDER_STATE GetLatOrderStateByGangOrderState(string gangHaoTaskState)
        {
            switch (gangHaoTaskState)
            {
                case "CREATED":
                    return ORDER_STATE.EXECUTING;
                case "TOBEDISPATCHED":
                    return ORDER_STATE.WAIT_EXECUTE;
                case "RUNNING":
                    return ORDER_STATE.EXECUTING;
                case "FINISHED":
                    return ORDER_STATE.COMPLETE;
                case "FAILED":
                    return ORDER_STATE.FAILED;
                case "STOPPED":
                    return ORDER_STATE.STOPPED;
                case "Error":
                    return ORDER_STATE.ERROR;
                case "WAITING":
                    return ORDER_STATE.WAIT_EXECUTE;
                default:
                    return ORDER_STATE.WAIT_EXECUTE;
            }
        }
    }
}
