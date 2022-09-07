using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public interface IAgvsApi
    {
        /// <summary>
        /// 上報車子狀態給派車
        /// </summary>
        /// <param name="agvcRunningStateData"></param>
        /// <returns></returns>
        bool RunningStatusReport(Dictionary<string, object> agvcRunningStateData);

        /// <summary>
        ///  回覆Task Download Result給派車
        /// </summary>
        /// <param name="agvc">被指派的AGVC</param>
        /// <param name="success">AGVC是否會執行訂單?</param>
        void TaskDownloadReport(IAGVC agvc, bool success);

        /// <summary>
        /// 回報訂單執行狀態給派車
        /// </summary>
        /// <param name="order"></param>
        void TaskStateFeedback(clsHostExecuting order);
        void ResetReport(IAGVC agvc, bool setOrderSuccess);
    }
}
