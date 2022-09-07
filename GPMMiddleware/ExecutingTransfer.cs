using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System.Threading.Tasks;
using GangHaoAGV.Models.Order;
using GangHaoAGV.API;
using GPM_AGV_LAT_CORE.LATSystem;

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
            internal static async Task<bool> TransferToGangHao(clsHostExecuting newExecuting)
            {
                var ExecuteType = newExecuting.EExecuteType;
                ServerAPI api = new ServerAPI() { baseUrl = SystemParams.GangHaoRDSCoreServerUrl };
                ResponseBase response = null;

                if (ExecuteType == ExcutingPreProcessor.EXECUTE_TYPE.Order)
                {
                    SetOrder gangOrder = OrderConverter.LATToAGVC.ToGangHaoOrder(newExecuting.latOrderDetail);
                    response = await api.SetOrder(gangOrder);
                }
                else if (ExecuteType == ExcutingPreProcessor.EXECUTE_TYPE.Reset)
                {
                    response = await api.TerminateAGVCurrentOrder(newExecuting.ExecuteingAGVC.agvcID, true);
                }

                return response == null ? false : (response.code == 0);
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
