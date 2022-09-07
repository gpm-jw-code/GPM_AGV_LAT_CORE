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

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    /// <summary>
    /// 處理AGVS事件
    /// </summary>
    internal partial class AgvsHandler
    {
        /// <summary>
        /// 處理晶捷能派車平台任務下載
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static async void KingGellantAGVSTaskDownloadHandle(object sender, object taskObject)
        {
            IAGVS agvs = (KingGallentAGVS)sender;
            IAgvsExcutingPreProcessor processor = new KingGallentExcutingPreProcessor();
            clsHostExecuting newOrder = processor.Run(agvs, taskObject);
            ExecutingTransferWorkFlow(agvs, processor.agvcFound, newOrder);
        }

        /// <summary>
        /// 處理GPM派車平台任務下載 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static void GPMAGVSTaskDownloadHandle(object sender, object taskObject)
        {
            IAGVS agvs = (IAGVS)sender;
            IAgvsExcutingPreProcessor processor = new GpmExcutingPreProcessor();
            clsHostExecuting newExecuting = processor.Run(agvs, taskObject);
            ExecutingTransferWorkFlow(agvs, processor.agvcFound, newExecuting);
        }


        /// <summary>
        /// 任務/訂單轉移流程
        /// </summary>
        /// <param name="agvs"></param>
        /// <param name="agvc"></param>
        /// <param name="newExecuting"></param>
        private static async void ExecutingTransferWorkFlow(IAGVS agvs, IAGVC agvc, clsHostExecuting newExecuting)
        {
            var executeType = newExecuting.EExecuteType;
            if (executeType == EXECUTE_TYPE.Order)
            {
                newExecuting = OrderManerger.NewOrderJoin(newExecuting);
                newExecuting.PropertyChanged += (order, e) => TaskStateFeedBack(agvs, order as clsHostExecuting);
            }
            bool setOrderSuccess = await TransferExecutingToAGVC(agvc, newExecuting);
            ExecutingResultReport(agvs, agvc, executeType, setOrderSuccess);
        }


        /// <summary>
        /// 回報
        /// </summary>
        /// <param name="agvs"></param>
        /// <param name="agvc"></param>
        /// <param name="executeType"></param>
        /// <param name="setOrderSuccess"></param>
        private static void ExecutingResultReport(IAGVS agvs, IAGVC agvc, EXECUTE_TYPE executeType, bool setOrderSuccess)
        {
            if (executeType == EXECUTE_TYPE.Order)
                agvs.agvsApi.TaskDownloadReport(agvc, setOrderSuccess);
            else if (executeType == EXECUTE_TYPE.Reset)
                agvs.agvsApi.ResetReport(agvc, setOrderSuccess);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="agvc"></param>
        /// <param name="newExecuting"></param>
        /// <returns></returns>
        private static async Task<bool> TransferExecutingToAGVC(IAGVC agvc, clsHostExecuting newExecuting)
        {
            bool setOrderSuccess = false;
            AGVC_TYPES agvcType = agvc.agvcType;
            if (agvcType == AGVC_TYPES.GangHau)
            {
                setOrderSuccess = await ExecutingTransfer.TransferToGangHao(newExecuting);
            }
            else if (agvcType == AGVC_TYPES.GPM)
            {
                setOrderSuccess = await ExecutingTransfer.TransferToGPM(newExecuting);
            }
            newExecuting.State = setOrderSuccess ? ORDER_STATE.EXECUTING : ORDER_STATE.WAIT_EXECUTE;

            if (setOrderSuccess && newExecuting.EExecuteType == EXECUTE_TYPE.Order)
                agvc.AddHostOrder(newExecuting);
            return setOrderSuccess;
        }



        private static void TaskStateFeedBack(IAGVS agvs, clsHostExecuting order)
        {
            if (order.State == ORDER_STATE.COMPLETE)
            {

            }
            agvs.agvsApi.TaskStateFeedback(order);
        }



    }
}
