using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.GPMMiddleware.S2CConverter;
using GPM_AGV_LAT_CORE.LATSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    /// <summary>
    /// 處理AGVS事件
    /// </summary>
    internal class AgvsHandler
    {
        internal static void HostMessageHandle(object sender, object hostData)
        {
            string hostSourceName = sender.GetType().Name;
            Console.WriteLine("HOST Message Rev-From TYPE: {0} | Data:{1}", hostSourceName, hostData);
            AGVSHostDataHandleWork(hostSourceName, hostData);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void AGVSHostDataHandleWork(string source, object hostData)
        {
            KingGallentAGVS king2G = new KingGallentAGVS();
            if (source == king2G.GetType().Name)
            {
                King2GPM(hostData);
            }
        }


        public static void King2GPM(object hostData)
        {
            Console.WriteLine("KingAllan Host Data will be Handled {0}", hostData);

        }



        /// <summary>
        /// 處理晶捷能派車平台任務下載
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskObject"></param>
        internal static void KingGellantTaskDownloadHandle(object sender, object taskObject)
        {
            KingGallentAGVS agvs = (KingGallentAGVS)sender;
            IAGVC agvc = null;

            Dictionary<string, object> _taskObj = (Dictionary<string, object>)taskObject;
            //解析
            string SID = _taskObj["SID"].ToString();
            string EQName = _taskObj["EQName"].ToString();
            agvc = FindAGVCInKingGallent(SID);

            Console.WriteLine("AGV TaskDownload From {0} to {1} |Task Content: {2}", agvs.agvsType, agvc.GetType().Name, JsonConvert.SerializeObject(taskObject));
            IS2Converter converter = ConverterSelect(agvs, agvc);
            clsHostOrder newOrder = new clsHostOrder()
            {
                RecieveTimeStamp = DateTime.Now,
                FromAGVS = agvs,
                ExecuteingAGVC = agvc,
                State = ORDER_STATE.WAIT_EXECUTE,
                TaskDownloadData = taskObject
            };

            OrderManerger.NewOrderJoin(newOrder);
            ///找到車子後把任務提交給車子確認接不接

            bool orderRecieved = agvc.TryExecuteOrder(newOrder, out string agvcMsg);

            ///模擬完成訂單
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                newOrder.State = ORDER_STATE.EXECUTING;
                await Task.Delay(TimeSpan.FromSeconds(4));
                newOrder.State = ORDER_STATE.COMPLETE;
            });

            converter.TaskDownloadConvert(taskObject);
        }


        private static IAGVC FindAGVCInKingGallent(string SID)
        {
            var agcList = AGVCManager.AGVCList.FindAll(agv => agv.agvcInfos.GetType().Name == "AgvcInfoForKingAllant");
            if (agcList.Count == 0)
            {
                Console.WriteLine("找不到任何車屬於晶捷能派車系統");
                return null;
            }
            var agvc = agcList.FirstOrDefault(agv => ((AgvcInfoForKingAllant)agv.agvcInfos).SID == SID);
            if (agvc == null)
            {
                Console.WriteLine("找不到任何車SID為{0}", SID);
                return null;
            }
            return agvc;
        }


        private static IS2Converter ConverterSelect(IAGVS agvs, IAGVC agvc)
        {
            IS2Converter converter = null;
            AGVC_TYPES agvcType = agvc.agvcType;
            AGVS_TYPES agvsType = agvs.agvsType;
            switch (agvsType)
            {
                case AGVS_TYPES.GPM:
                    switch (agvcType)
                    {
                        case AGVC_TYPES.GPM:
                            converter = new GPMAgvs2GPMAgvcConverter();
                            break;
                        case AGVC_TYPES.GangHau:
                            converter = new GPMAgvs2GangHaoAgvcConverter();
                            break;
                        default:
                            break;
                    }
                    break;
                case AGVS_TYPES.KINGGALLENT:
                    switch (agvcType)
                    {
                        case AGVC_TYPES.GPM:
                            converter = new KingGallentAgvs2GPMAgvcConverter();
                            break;
                        case AGVC_TYPES.GangHau:
                            converter = new KingGallentAgvs2GangHaoAgvcConverter();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return converter;
        }
    }
}
