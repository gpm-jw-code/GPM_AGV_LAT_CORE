using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{

    /// <summary>
    /// 處理AGVC事件
    /// </summary>
    public static class AgvcHandler
    {
        static ILogger logger = new LoggerInstance(typeof(AgvcHandler));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">IAGVC</param>
        /// <param name="latStateStore">AGVCStateStore</param>
        internal static void StateOnChangedHandler(object sender, AGVCStateStore latStateStore)
        {
            IAGVC agvc = (IAGVC)sender;
            IAGVS agvsBinding = agvc.agvsBinding;
            agvsBinding.ReportAGVCState(agvc, latStateStore);
        }

        internal static void OrderStateOnChangeHandler(object sender, EventArgs e)
        {

        }

        internal static void CheckOnlineStateHandler(object sender, IAGVC agvc)
        {
            var onlineState = agvc.agvsBinding.agvsApi.DownloadAgvcOnlineState(agvc);
            logger.InfoLog($"agvc-{agvc.EQName} online State download result : {onlineState}");
            agvc.agvcStates.States.EOnlineState = onlineState;
        }
    }
}
