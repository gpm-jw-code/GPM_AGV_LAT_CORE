using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GangHaoAGV.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.GPMMiddleware.ExcutingPreProcessor;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl;
using GangHaoAGV.AGV;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    /// <summary>
    /// 處理AGVS事件
    /// </summary>
    internal partial class AgvsHandler
    {


        static ILogger logger = new LoggerInstance(typeof(AgvsHandler));
        /// <summary>
        /// 處理晶捷能派車平台任務下載
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static async void KingGellantAGVSTaskDownloadHandle(object sender, IAGVSExecutingState executingState)
        {
            IAGVS agvs = (KingGallentAGVS)sender;
            IAgvsExcutingPreProcessor processor = new KingGallentExcutingPreProcessor();
            clsHostExecuting newOrder = processor.Run(agvs, executingState);
            ExecutingTransferWorkFlow(agvs, processor.agvcFound, newOrder, executingState);
        }

        /// <summary>
        /// 處理GPM派車平台任務下載 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static void GPMAGVSTaskDownloadHandle(object sender, IAGVSExecutingState executingState)
        {
            IAGVS agvs = (IAGVS)sender;
            IAgvsExcutingPreProcessor processor = new GpmExcutingPreProcessor();
            clsHostExecuting newExecuting = processor.Run(agvs, executingState);
            ExecutingTransferWorkFlow(agvs, processor.agvcFound, newExecuting, executingState);
        }


        /// <summary>
        /// 任務/訂單轉移流程
        /// </summary>
        /// <param name="agvs"></param>
        /// <param name="agvc"></param>
        /// <param name="newExecuting"></param>
        private static async void ExecutingTransferWorkFlow(IAGVS agvs, IAGVC agvc, clsHostExecuting newExecuting, IAGVSExecutingState executingState)
        {

            agvs.ExecuteTaskList.Add(newExecuting);

            var executeType = newExecuting.EExecuteType;
            if (executeType == EXECUTE_TYPE.Order)
            {
                newExecuting = OrderManerger.NewOrderJoin(newExecuting);
                //newExecuting.PropertyChanged += (order, e) => agvs.agvsApi.TaskStateFeedback(order as clsHostExecuting); ;
            }
            var transferResult = await TransferExecutingToAGVC(agvc, newExecuting);

            executingState.state = newExecuting.State = transferResult.Success ? ORDER_STATE.EXECUTING : ORDER_STATE.WAIT_EXECUTE;

            ExecutingResultReport(agvs, agvc, executeType, transferResult.Success);

            TaskOrderStateTrack orderStateTracker = new TaskOrderStateTrack(agvs, agvc, newExecuting);

            if (transferResult.Success)
            {
                if (newExecuting.EExecuteType == EXECUTE_TYPE.Order)
                {

                    TrafficControlCenter.JoinTrafficSystem(agvc, newExecuting.latOrderDetail.action.paths);
                    orderStateTracker.StartTrack();
                }
            }
            else
            {
                logger.WarnLog($"任務發派失敗:{transferResult.ErrMessage}");
            }
        }


        /// <summary>
        /// 回報任務
        /// </summary>
        /// <param name="agvs"></param>
        /// <param name="agvc"></param>
        /// <param name="executeType"></param>
        /// <param name="setOrderSuccess"></param>
        private static void ExecutingResultReport(IAGVS agvs, IAGVC agvc, EXECUTE_TYPE executeType, bool setOrderSuccess, IAGVSExecutingState executingState = null)
        {
            if (executeType == EXECUTE_TYPE.Order)
                agvs.agvsApi.ReportTaskDownloadResult(agvc, setOrderSuccess, executingState);
            else if (executeType == EXECUTE_TYPE.Reset)
                agvs.agvsApi.ReportNagivateResetExecuteResult(agvc, setOrderSuccess, executingState);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="agvc"></param>
        /// <param name="newExecuting"></param>
        /// <returns></returns>
        private static async Task<TransferResult> TransferExecutingToAGVC(IAGVC agvc, clsHostExecuting newExecuting)
        {
            try
            {
                bool setOrderSuccess = false;
                string message = "";
                AGVC_TYPES agvcType = agvc.agvcType;
                if (agvcType == AGVC_TYPES.GangHau)
                {
                    clsMap.NavigateReqResult res = await ExecutingTransfer.TransferToGangHao(newExecuting);
                    setOrderSuccess = res.Success;
                    message = res.ErrMsg;

                    if (newExecuting.EExecuteType == EXECUTE_TYPE.Order)
                        newExecuting.latOrderDetail.action.paths = res.Path?.ToList();
                }
                else if (agvcType == AGVC_TYPES.GPM)
                {
                    setOrderSuccess = await ExecutingTransfer.TransferToGPM(newExecuting);
                }

                //newExecuting.State = setOrderSuccess ? ORDER_STATE.EXECUTING : ORDER_STATE.WAIT_EXECUTE;
                logger.TraceLog($"AGVC |{agvc.EQName}| 執行任務-check-pt1 : {setOrderSuccess}");
                if (setOrderSuccess && newExecuting.EExecuteType == EXECUTE_TYPE.Order)
                    agvc.AddHostOrder(newExecuting);
                return new TransferResult(setOrderSuccess)
                {
                    ErrMessage = message
                };
            }
            catch (Exception ex)
            {
                return new TransferResult(false)
                {
                    ErrMessage = $"Code Error : {ex.Message}"
                };
            }
        }



    }

    public class TransferResult
    {
        public TransferResult(bool Success)
        {
            this.Success = Success;
        }
        public bool Success { get; set; }
        public string ErrMessage { get; set; }
    }
}
