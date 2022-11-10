using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{

    /// <summary>
    /// 處理AGVC事件
    /// </summary>
    public static class AgvcHandler
    {
        public static ManualResetEvent ReportPauseResetEvent = new ManualResetEvent(true);
        static ILogger logger = new LoggerInstance(typeof(AgvcHandler));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">IAGVC</param>
        /// <param name="latStateStore">AGVCStateStore</param>
        internal static void StateOnChangedHandler(object sender, AGVCStateStore latStateStore)
        {
            //if (Debugger.IsAttached)
            //    return;
            Task.Run(() =>
            {
                ReportPauseResetEvent.WaitOne();
                IAGVC agvc = (IAGVC)sender;
                IAGVS agvsBinding = agvc.agvsBinding;
                agvsBinding.ReportAGVCState(agvc, latStateStore);
            });
        }

        internal static void OrderStateOnChangeHandler(object sender, EventArgs e)
        {

        }

        internal async static void CheckOnlineStateHandler(object sender, IAGVC agvc)
        {
            ReportPauseResetEvent.Reset();
            Thread.Sleep(1000);
            try
            {
                var onlineState = agvc.agvsBinding.agvsApi?.DownloadAgvcOnlineState(agvc).Result;
                logger.InfoLog($"agvc-{agvc.EQName} online State download result : {onlineState}");
                agvc.agvcStates.States.EOnlineState = onlineState;
            }
            catch (Exception ex)
            {
                logger.ErrorLog(ex);
            }
            ReportPauseResetEvent.Set();
        }

        /// <summary>
        /// 要求上/下線
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="state"></param>
        internal async static void OnlineOffLineRequestHandler(object sender, AGVCBase.OnOffLineRequest state)
        {
            ReportPauseResetEvent.Reset();
            await Task.Delay(1000);
            IAGVC agvc = state.agvc;
            ONLINE_STATE stateReq = state.stateReq;
            logger.InfoLog($"[Online/Offline Request Handle]{agvc.EQName} 要求 {stateReq.ToString()}");

            var currentOnlineState = agvc.agvsBinding.agvsApi?.DownloadAgvcOnlineState(agvc).Result;
            if (state.stateReq == currentOnlineState)
            {
                agvc.agvcStates.States.EOnlineState = currentOnlineState;
                logger.InfoLog($"[Online/Offline Request Handle]{agvc.EQName} 上線狀態現在是 {currentOnlineState.ToString()}");
                return;
            }


            var onlineState = state.agvc.agvsBinding.agvsApi.AgvcOnOffLineRequst(agvc, stateReq, state.currentStation).Result;
            agvc.agvcStates.States.EOnlineState = onlineState;
            logger.InfoLog($"[Online/Offline Request Handle]{agvc.EQName} 上線狀態現在是 {onlineState.ToString()}");
            if (state.stateReq == ONLINE_STATE.OFFLINE)
                ReportPauseResetEvent.Set();
            else if (state.stateReq == ONLINE_STATE.ONLINE)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(8000);
                    ReportPauseResetEvent.Set();
                });
            }
        }
    }
}
