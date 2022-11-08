using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators.KingGallentAGVS
{

    public partial class KingGallentAgvsEmulator
    {
        public class BindingAGVC
        {

            public event EventHandler<TaskDownObject> OnTaskDownload;

            public BindingAGVC(string EQName, string SID)
            {
                this.EQName = EQName;
                this.SID = SID;
            }

            public string SID { get; set; }
            public string EQName { get; set; }
            public OrderTask ExecutingOrder { get; private set; } = null;
            public string ExecutingTaskName { get; private set; } = null;
            public Queue<OrderTask> waintingOrderLinks { get; private set; } = new Queue<OrderTask>();
            public MapState.MapInfo mapInfo { get; internal set; } = new MapState.MapInfo();

            private ManualResetEvent OrderTaskResetEvent = new ManualResetEvent(false);
            private CancellationTokenSource NavigatingCancelTokenSource = new CancellationTokenSource();

            internal OrderResult NewOrder(OrderTask order, bool waitOtherTaskFinish)
            {
                if (ExecutingOrder != null && waitOtherTaskFinish) //當有任務鍊在進行中 但接受等待
                {
                    waintingOrderLinks.Enqueue(order);
                    return new OrderResult(true, OrderResult.RUN_STATE.WAITING);
                }
                OrderTaskResetEvent = new ManualResetEvent(true);
                _ = OrderLinkRun(order);
                return new OrderResult(true, OrderResult.RUN_STATE.EXECUTING);
            }


            private async Task OrderLinkRun(OrderTask order)
            {
                NavigatingCancelTokenSource = new CancellationTokenSource();
                ExecutingOrder = order;

                Task tk = await Task.Factory.StartNew(async () =>
                 {
                     try
                     {
                         while (order.StationsQueue.Count != 0)
                         {
                             OrderTaskResetEvent.Reset();//封鎖
                             OrderTask.StationInfo station = order.StationsQueue.Dequeue();
                             ExecutingTaskName = order.TaskID + $"station-{station.stationID}";
                             OnTaskDownload?.Invoke(this, new TaskDownObject { SID = this.SID, EQName = this.EQName, taskName = ExecutingTaskName, stationID = station.stationID });
                             OrderTaskResetEvent.WaitOne();

                         }

                         ExecutingOrder = null;
                         ExecutingTaskName = null;

                         if (waintingOrderLinks.Count != 0)
                         {
                             OrderTask nextExecuting = waintingOrderLinks.Dequeue();
                             Task.Factory.StartNew(() => OrderLinkRun(nextExecuting));
                         }
                     }
                     catch (Exception ex)
                     {
                         Console.WriteLine(ex.Message);
                     }


                 }, NavigatingCancelTokenSource.Token);

            }

            internal void SetWorkFlowResume()
            {
                OrderTaskResetEvent.Set();
            }

            internal void CancelNavigating()
            {
                waintingOrderLinks.Clear();
                ExecutingOrder?.StationsQueue.Clear();
                NavigatingCancelTokenSource.Cancel();
                ExecutingOrder = null;
            }
        }


    }
}
