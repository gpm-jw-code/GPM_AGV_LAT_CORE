using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System.Threading.Tasks;
using GangHaoAGV.API;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.AGVC;
using GangHaoAGV.Models.MapModels.Requests;
using System.Collections.Generic;
using static GangHaoAGV.AGV.clsMap;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    internal partial class AgvsHandler
    {
        /// <summary>
        /// 將訂單(LAT物件/格式)送給AGVC執行
        /// </summary>
        internal struct ExecutingTransfer
        {

            /// <summary>
            /// 送給罡豪AGVC 執行訂單
            /// </summary>
            /// <param name="newExecuting"></param>
            /// <returns></returns>
            internal static async Task<NavigateReqResult> TransferToGangHao(clsHostExecuting newExecuting)
            {
                GangHaoAGVC haoAGVC = (GangHaoAGVC)newExecuting.ExecutingAGVC;
                NavigateReqResult response = null;
                var ExecuteType = newExecuting.EExecuteType;

                if (ExecuteType == ExcutingPreProcessor.EXECUTE_TYPE.Order)
                {
                    //haoAGVC.AGVInterface.
                    //robotMapTaskGoTargetListReq_3066 task_3066 = OrderConverter.LATToAGVC.ToGangHaoOrder(newExecuting.latOrderDetail);
                    var task_3051 = OrderConverter.LATToAGVC.ToGanHaoOrder.ToGoTargetOrder(newExecuting.latOrderDetail);
                    response = await haoAGVC.AGVInterface.NAVIGATIOR.GoTarget(task_3051);

                }
                else if (ExecuteType == ExcutingPreProcessor.EXECUTE_TYPE.Reset)
                {
                    response = new NavigateReqResult();
                    response.Success = await haoAGVC.AGVInterface.NAVIGATIOR.TaskCancel();
                }

                return response;
            }

            /// <summary>
            /// 送給GPM AGVC 執行訂單
            /// </summary>
            /// <param name="newOrder"></param>
            /// <returns></returns>
            internal static async Task<bool> TransferToGPM(clsHostExecuting newOrder)
            {
                //TODO ORDER Transfer To GPM AGVC
                OrderConverter.LATToAGVC.ToGPMOrder(newOrder.latOrderDetail);
                return true;
            }

        }


    }
}
