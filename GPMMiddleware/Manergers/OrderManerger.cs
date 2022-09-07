using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers
{
    /// <summary>
    /// 這是派車平台訂單管理員
    /// </summary>
    public static class OrderManerger
    {

        public static event EventHandler<clsHostExecuting> OnNewOrderCreate;

        public static List<clsHostExecuting> OrderList { get; private set; } = new List<clsHostExecuting>();

        /// <summary>
        /// 加入一筆派車訂單
        /// </summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        internal static clsHostExecuting NewOrderJoin(clsHostExecuting newOrder)
        {
            newOrder.PropertyChanged += NewOrder_PropertyChanged;
            //定流水號
            newOrder.OrderNo = OrderList.Count;
            newOrder.State = ORDER_STATE.WAIT_EXECUTE;
            OrderList.Add(newOrder);

            OnNewOrderCreate?.Invoke("OrderManerger", newOrder);
            return newOrder;
        }

        private static void NewOrder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnNewOrderCreate?.Invoke("OrderManerger", (clsHostExecuting)sender);
        }


    }
}
