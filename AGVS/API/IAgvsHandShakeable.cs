using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public interface IAgvsHandShakeable
    {
        /// <summary>
        /// 上報車子狀態給派車
        /// </summary>
        /// <param name="agvcRunningStateData"></param>
        /// <returns></returns>
        bool RunningStatusReport(Dictionary<string, object> agvcRunningStateData);

        /// <summary>
        /// [被動回覆] 回覆Task Download Result給派車
        /// </summary>
        /// <param name="agvc">被指派的AGVC</param>
        /// <param name="success">AGVC是否會執行訂單?</param>
        void TaskDownloadReport(IAGVC agvc, bool success, IAGVSExecutingState executingState);
        /// <summary>
        /// [被動回覆] 回覆Task Download Result給派車
        /// </summary>
        /// <param name="agvc"></param>
        /// <param name="setOrderSuccess"></param>
        /// <param name="executingState"></param>
        void ResetReport(IAGVC agvc, bool setOrderSuccess, IAGVSExecutingState executingState);

        /// <summary>
        /// [主動] 回報訂單執行狀態給派車
        /// </summary>
        /// <param name="order"></param>
        void TaskStateFeedback(clsHostExecuting order);

        /// <summary>
        /// 從派車平台下載車子的上下線狀態
        /// </summary>
        /// <param name="agvc"></param>
        /// <returns></returns>
        ONLINE_STATE DownloadAgvcOnlineState(IAGVC agvc);

        ONLINE_STATE AgvcOnOffLineRequst(IAGVC agvc, ONLINE_STATE stateReq);
    }
}
