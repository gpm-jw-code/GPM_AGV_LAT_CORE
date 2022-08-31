using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.S2CConverter;
using GPM_AGV_LAT_CORE.LATSystem;
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

        internal static void TaskDownloadHandle(object sender, object taskObject)
        {
            IAGVS agvs = (IAGVS)sender;
            IAGVC agvc = null;
            if (agvs.agvsType == LATSystem.AGVS_TYPES.KINGGALLENT)
            {
                Dictionary<string, object> _taskObj = (Dictionary<string, object>)taskObject;
                //解析
                string SID = _taskObj["SID"].ToString();
                string EQName = _taskObj["EQName"].ToString();
                Console.WriteLine("AGV(SID:{0}/EQName:{1}) TaskDownload: {2}", SID, EQName, agvc);
                agvc = FindAGVCInKingGallent(SID);
            }

            IS2Converter converter = ConverterSelect(agvs, agvc);
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
