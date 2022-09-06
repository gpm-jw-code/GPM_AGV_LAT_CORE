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
        /// 上報車子狀態
        /// </summary>
        /// <param name="agvcRunningStateData"></param>
        /// <returns></returns>
        bool RunningStatusReport(Dictionary<string, object> agvcRunningStateData);

        /// <summary>
        /// 回覆Task Download Result
        /// </summary>
        /// <param name="SID"></param>
        /// <param name="EQName"></param>
        /// <param name="returnCode"></param>
        void TaskDownloadReport(string SID, string EQName, int returnCode);

        void TaskStateFeedback(clsHostOrder order);
    }
}
