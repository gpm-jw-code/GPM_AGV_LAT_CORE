using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Linq;
using GangHaoAGV.Models.Order;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    internal partial class AgvsHandler
    {
        /// <summary>
        /// AGVS->LAT->AGVC 訂單轉換
        /// </summary>
        internal struct OrderConverter
        {
            /// <summary>
            /// 將[LAT]訂單格式轉成[AGVC]訂單格式
            /// </summary>
            internal struct LATToAGVC
            {
                /// <summary>
                /// 轉成 罡豪 AGVC 訂單格式
                /// </summary>
                /// <param name="latOrder"></param>
                /// <returns></returns>
                internal static SetOrder ToGangHaoOrder(clsLATOrderDetail latOrder)
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
                /// <summary>
                /// 轉成 GPM AGVC 訂單格式
                /// </summary>
                /// <param name="latOrderDetail"></param>
                /// <exception cref="NotImplementedException"></exception>
                internal static void ToGPMOrder(clsLATOrderDetail latOrderDetail)
                {
                    throw new NotImplementedException();
                }
            }
            /// <summary>
            /// 將[AGVS]訂單格式轉成[LAT]訂單格式
            /// </summary>
            internal struct AGVSToLAT
            {
                /// <summary>
                /// 把KingGallent Download Task 物件轉成LAT格式
                /// </summary>
                /// <param name="orderObject"></param>
                /// <returns></returns>
                internal static clsLATOrderDetail KingGallentOrderToLATOrder(Dictionary<string, object> orderObject)
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
                /// <summary>
                /// 把GPM Download Task 物件轉成LAT格式
                /// </summary>
                /// <param name="taskObject"></param>
                /// <returns></returns>
                /// <exception cref="NotImplementedException"></exception>
                internal static clsLATOrderDetail GPMOrderToLATOrder(object taskObject)
                {
                    throw new NotImplementedException();
                }

            }
        }


    }
}
