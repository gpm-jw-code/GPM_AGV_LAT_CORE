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
using GangHaoAGV.Models.Order;
using GangHaoAGV.API;
using GPM_AGV_LAT_CORE.AGVS.API;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    /// <summary>
    /// 處理AGVS事件
    /// </summary>
    internal class AgvsHandler
    {
        /// <summary>
        /// 處理晶捷能派車平台任務下載
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static async void KingGellantTaskDownloadHandle(object sender, object taskObject)
        {
            KingGallentAGVS agvs = (KingGallentAGVS)sender;

            IAGVC agvc = null;
            Dictionary<string, object> _taskObj = (Dictionary<string, object>)taskObject;
            //解析
            string SID = _taskObj["SID"].ToString();
            string EQName = _taskObj["EQName"].ToString();
            agvc = AGVCManager.FindAGVCInKingGallentBySID(SID);

            //轉成LAT Format
            clsLATOrderDetail latOrder = KingGallentOrderToLATOrder(_taskObj);
            Console.WriteLine("AGV TaskDownload From {0} to {1} |Task Content: {2}", agvs.agvsType, agvc == null ? "No-Car-Match" : agvc.GetType().Name, JsonConvert.SerializeObject(taskObject));
            clsHostOrder newOrder = new clsHostOrder(agvs, agvc, taskObject)
            {
                RecieveTimeStamp = DateTime.Now,
                State = ORDER_STATE.WAIT_EXECUTE,
                latOrderDetail = latOrder
            };

            newOrder = OrderManerger.NewOrderJoin(newOrder);
            newOrder.PropertyChanged += (order, e) => TaskStateFeedBack(agvs, order as clsHostOrder);

            if (agvc != null)
            {
                bool setOrderSuccess = false;

                if (agvc.agvcType == LATSystem.AGVC_TYPES.GangHau) //罡豪車用 call api的方式派車執行任務
                {
                    SetOrder gangOrder = LATOrderToGangHaoOrder(latOrder);
                    ServerAPI api = new ServerAPI() { baseUrl = "http://localhost:5279/api/Server" };
                    ResponseBase response = await api.SetOrder(gangOrder);
                    setOrderSuccess = response.code == 0;
                    newOrder.State = setOrderSuccess ? ORDER_STATE.EXECUTING : ORDER_STATE.WAIT_EXECUTE;
                }
                agvc.AddHostOrder(newOrder);
                agvs.agvsApi.TaskDownloadReport(SID, EQName, setOrderSuccess ? 0 : 400);
            }
            else
                Console.WriteLine("這訂單沒人接");

        }

        private static void TaskStateFeedBack(IAGVS agvs, clsHostOrder order)
        {
            if(order.State== ORDER_STATE.COMPLETE)
            {

            }
            agvs.agvsApi.TaskStateFeedback(order);
        }

        /// <summary>
        /// 把KingGallent Download Task 物件轉成LAT格式
        /// </summary>
        /// <param name="orderObject"></param>
        /// <returns></returns>
        private static clsLATOrderDetail KingGallentOrderToLATOrder(Dictionary<string, object> orderObject)
        {

            var headerObj = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(orderObject["Header"].ToString());
            if (headerObj.Count == 0)
            {
                return null;
            }

            var taskItem = headerObj.First().Value;

            string _taskName = taskItem["Task Name"].ToString();
            string _eqName = orderObject["EQName"].ToString();
            //TODO 解析actions (路徑)
            string _action1_name = _taskName + "-block1";
            return new clsLATOrderDetail()
            {
                taskName = _taskName,
                complete = false,
                executEqName = _eqName,
                actions = new List<clsLATOrderDetail.clsAction>()
                {
                        new clsLATOrderDetail.clsAction(){
                             actionID = _action1_name,
                             location ="PA1"
                        },
                }
            };
        }


        private static SetOrder LATOrderToGangHaoOrder(clsLATOrderDetail latOrder)
        {
            return new SetOrder()
            {
                id = latOrder.taskName,
                complete = latOrder.complete,
                priority = 999,
                vehicle = latOrder.executEqName,
                blocks = latOrder.actions.Select(act => new Block()
                {
                    blockId = act.actionID,
                    location = act.location,
                    operationArgs = act.operation_args
                }).ToList()
            };
        }


    }
}
