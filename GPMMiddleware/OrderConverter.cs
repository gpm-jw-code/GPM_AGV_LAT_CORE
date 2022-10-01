using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using GangHaoAGV.Models.MapModels.Requests;

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

                internal struct ToGanHaoOrder
                {
                    ///// <summary>
                    ///// 轉成 罡豪 AGVC 訂單格式
                    ///// </summary>
                    ///// <param name="latOrder"></param>
                    ///// <returns></returns>
                    //internal static robotMapTaskGoTargetListReq_3066 ToGoTargetListOrder(List<clsLATTaskOrder> latOrder)
                    //{
                    //    List<clsLATTaskOrder.clsAction> actions = latOrder.actions;
                    //    string taskID = latOrder.taskName;
                    //    List<robotMapTaskGoTargetReq_3051> targetList = new List<robotMapTaskGoTargetReq_3051>();
                    //    for (int i = 1; i < actions.Count; i++)
                    //    {
                    //        robotMapTaskGoTargetReq_3051 target = new robotMapTaskGoTargetReq_3051()
                    //        {
                    //            source_id = actions[i - 1].stationID,
                    //            id = actions[i].stationID,
                    //            task_id = taskID + $"-{i}"

                    //        };
                    //        targetList.Add(target);
                    //    }

                    //    return new robotMapTaskGoTargetListReq_3066()
                    //    {
                    //        move_task_list = targetList
                    //    };
                    //}

                    internal static robotMapTaskGoTargetReq_3051 ToGoTargetOrder(clsLATTaskOrder latOrder)
                    {
                        return new robotMapTaskGoTargetReq_3051
                        {
                            task_id = latOrder.taskName,
                            id = latOrder.action.stationID,
                            source_id = "SELF_POSITION"
                        };
                    }

                }

                /// <summary>
                /// 轉成 GPM AGVC 訂單格式
                /// </summary>
                /// <param name="latOrderDetail"></param>
                /// <exception cref="NotImplementedException"></exception>
                internal static void ToGPMOrder(clsLATTaskOrder latOrderDetail)
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
                internal static clsLATTaskOrder KingGallentOrderToLATOrder(Dictionary<string, object> orderObject)
                {

                    var headerObj = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(orderObject["Header"].ToString());
                    if (headerObj.Count == 0)
                    {
                        return null;
                    }
                    var taskItem = headerObj.First().Value;
                    string _eqName = orderObject["EQName"].ToString();
                    string _taskName = taskItem["Task Name"].ToString();
                    //TODO 解析actions (路徑)
                    string trajectoryJson = taskItem["Trajectory"].ToString();
                    var trajectory = JsonConvert.DeserializeObject < Dictionary<string, Dictionary<string, object>>>(trajectoryJson);

                    return new clsLATTaskOrder()
                    {
                        taskName = _taskName,
                        executEqName = _eqName,
                        action = new clsLATTaskOrder.clsAction()
                        {
                            actionID = _taskName + "-action",
                            stationID = trajectory.Keys.First()
                        }
                    };
                }
                /// <summary>
                /// 把GPM Download Task 物件轉成LAT格式
                /// </summary>
                /// <param name="taskObject"></param>
                /// <returns></returns>
                /// <exception cref="NotImplementedException"></exception>
                internal static clsLATTaskOrder GPMOrderToLATOrder(object taskObject)
                {
                    throw new NotImplementedException();
                }

            }
        }


    }
}
