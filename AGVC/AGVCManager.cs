using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class AGVCManager
    {
        /// <summary>
        /// Key: AGV廠商 Value: AGV列表
        /// </summary>
        public static List<IAGVC> AGVCList = new List<IAGVC>()
        {
           new GangHaoAGVC() {ID="0001",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.233"} }},
           new GangHaoAGVC() {ID="0002",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.111"} }},
           new GangHaoAGVC() {ID="0003",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.112"} }},
        };

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
            }
        }
        /// <summary>
        /// 與所有的AGV車連線喔
        /// </summary>
        public static void ConnectToAGVCs()
        {
            foreach (IAGVC agvc in AGVCList.FindAll(agvc => agvc.agvcInfos != null))
            {
                agvc.ConnectToAGV();
                agvc.SyncState();
                agvc.SyncOrdersState();
                agvc.SyncSyncOrderExecuteState();
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
            var agvc = agcList.FirstOrDefault(agv => ((AgvcInfoForKingAllant)agv.agvcInfos).SID == SID);
            if (agvc == null)
            {
                Console.WriteLine("找不到任何車SID為{0}", SID);
                return null;
            }
            return agvc;
        }

    }
}
