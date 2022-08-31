using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public enum ORDER_STATE
    {
        /// <summary>
        /// 等待AGVC接收
        /// </summary>
        WAIT_EXECUTE,
        /// <summary>
        /// 已經被AGCV拿去執行
        /// </summary>
        EXECUTING,
        /// <summary>
        /// 已完成
        /// </summary>
        COMPLETE
    }
}
