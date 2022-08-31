using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
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

        public static event EventHandler<clsHostOrder> OnNewOrderCreate;

        public static List<clsHostOrder> OrderList { get; private set; } = new List<clsHostOrder>();

        internal static clsHostOrder NewOrderJoin(clsHostOrder newOrder)
        {
            newOrder.PropertyChanged += NewOrder_PropertyChanged;
            //定流水號
            newOrder.OrderNo = OrderList.Count;
            OrderList.Add(newOrder);
            OnNewOrderCreate?.Invoke("OrderManerger", newOrder);
            return newOrder;
        }

        private static void NewOrder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnNewOrderCreate?.Invoke("OrderManerger", (clsHostOrder)sender);
        }
    }
}
