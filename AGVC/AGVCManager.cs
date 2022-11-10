using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class AGVCManager
    {
        static ILogger logger = new LoggerInstance(typeof(AGVCManager));
        /// <summary>
        ///  AGV列表
        /// </summary>
        public static List<IAGVC> AGVCList = new List<IAGVC>()
        {
           new GangHaoAGVC() {ID="001",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.107"} }},
           //new GangHaoAGVC() {ID="002",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.1.215"} }},
           //new GangHaoAGVC() {ID="003",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.1.227"} }},
        };


        internal static List<IAGVC> GetNavigatingAGVCs()
        {
            return AGVCList.FindAll(agv => agv.agvcStates.MapStates.navigationState.IsNavigating);
        }

        /// <summary>
        /// 取得所有IDLE狀態的AGV
        /// </summary>
        public static List<IAGVC> IdlingAGVList
        {
            get
            {
                return AGVCList.FindAll(agv => agv.agvcStates.States.ERunningState == RUNNING_STATE.IDLE);
            }
        }

        internal static void EventsRegist()
        {
            foreach (IAGVC agvc in AGVCList)
            {
                agvc.StateOnChanged += AgvcHandler.StateOnChangedHandler;
                agvc.OrderStateOnChnaged += AgvcHandler.OrderStateOnChangeHandler;
                agvc.CheckOnlineStateFromAGVSRequest += AgvcHandler.CheckOnlineStateHandler;
                agvc.OnlineOfflineRequest += AgvcHandler.OnlineOffLineRequestHandler;
            }
        }

        public static IAGVC GetAGVCByEqName(string eqName)
        {
            return AGVCList.FirstOrDefault(agv => agv.EQName == eqName);
        }

        public static async Task<dynamic> GetAgvcNativeDataByEqName(string eqName)
        {
            IAGVC agvc = GetAGVCByEqName(eqName);
            if (agvc == null)
                return "";

            dynamic data = new ExpandoObject();
            if (agvc.agvcType == AGVC_TYPES.GangHau)
            {
                GangHaoAGVC gagvc = agvc as GangHaoAGVC;
                return gagvc.AGVInterface.STATES.NativeDatas;
            }


            return data;


        }

        internal static List<IAGVC> FindAGVCByCurrentPathIncludeStation(string stationID)
        {
            return AGVCList.FindAll(agv => agv.agvcStates.MapStates.navigationState.IsNavigating && agv.orderList_LAT.Last().latOrderDetail.action.paths.Contains(stationID));
        }

        public static async Task<object> GetAlarmStateByEqName(string eqName)
        {
            IAGVC agvc = GetAGVCByEqName(eqName);
            return await agvc.GetNativeAlarmState();
        }

        public static List<IAGVC> GetAGVCByMapName(string mapName)
        {
            return AGVCList.FindAll(agv => agv.agvcStates.MapStates.currentMapInfo.name == mapName);
        }

        public static List<string> GetMapNames()
        {
            List<string> mapNameList = new List<string>();
            foreach (var agvc in AGVCList)
            {
                mapNameList.AddRange(agvc.GetMapNames());
            }
            return mapNameList.Distinct().ToList();
        }



        /// <summary>
        /// 與所有的AGV車連線喔
        /// </summary>
        public static void ConnectToAGVCs()
        {
            foreach (IAGVC agvc in AGVCList.FindAll(agvc => agvc.agvcInfos != null))
            {
                Task.Run(async () =>
                {
                    bool connected = agvc.ConnectToAGV();
                    if (connected)
                    {
                        logger.InfoLog($"AGVC-{agvc.EQName} Connected");
                        await Task.Delay(1000);
                        if (!Debugger.IsAttached)
                        {
                            logger.InfoLog($"{agvc.EQName}>>執行重定位");
                            bool reloc_success = await agvc.RelocProcess();
                            logger.InfoLog($"{agvc.EQName}>>執行重定位 {(reloc_success ? "成功" : "失敗")}");
                        }
                        agvc.OnlineStateInitProcess();
                    }
                    else
                        logger.FatalLog($"AGVC-{agvc.EQName} Connect fail...", new Exception("agvc connected fail"));

                });
            }
        }
        internal static void StartStateAsync()
        {
            foreach (IAGVC agvc in AGVCList.FindAll(agvc => agvc.agvcInfos != null))
            {
                Task.Run(() => agvc.SyncState());
                Task.Run(() => agvc.SyncOrdersState());
                Task.Run(() => agvc.SyncSyncOrderExecuteState());
            }
        }

        public static int AGVCInfoBinding()
        {
            int bindedAgvcNum = 0;
            foreach (var item in AGVSManager.CurrentAGVS.BindingAGVCInfoList)
            {
                var agvcSelected = AGVCList.FirstOrDefault(agvc => agvc.agvcInfos == null);
                if (agvcSelected != null)
                {
                    agvcSelected.agvcInfos = item;
                    agvcSelected.agvsBinding = AGVSManager.CurrentAGVS;
                    bindedAgvcNum++;
                }
            }
            Console.WriteLine("綁定了 {0} 台車", bindedAgvcNum);
            return bindedAgvcNum;
        }

        internal static IAGVC FindAGVCInGPM()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根據SID找到一台被晶捷能派車系統註冊的車子
        /// </summary>
        /// <param name="SID"></param>
        /// <returns></returns>
        public static IAGVC FindAGVCInKingGallentBySID(string SID)
        {
            var agcList = AGVCManager.AGVCList.FindAll(agv => agv.agvcInfos.GetType().Name == "AgvcInfoForKingAllant");
            if (agcList.Count == 0)
            {
                Console.WriteLine("找不到任何車屬於晶捷能派車系統");
                return null;
            }
            var agvc = agcList.FirstOrDefault(agv => ((AgvcInfoForKingAllant)agv.agvcInfos).EQName == SID | ((AgvcInfoForKingAllant)agv.agvcInfos).EQName.Replace("0", "") == SID);
            if (agvc == null)
            {
                Console.WriteLine("找不到任何車SID為{0}", SID);
                return null;
            }
            return agvc;
        }

    }
}
