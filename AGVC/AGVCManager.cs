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
           new GangHaoAGV() {ID="0001",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.233"} }},
           new GangHaoAGV() {ID="0002",  agvcParameters =new Parameters.AGVCParameters{tcpParams = new Parameters.TCPParameters{HostIP="192.168.0.111"} }},
        };


        /// <summary>
        /// 取得罡豪AGV車
        /// </summary>
        public static List<IAGVC> GangHaoAGVList => AGVCList.FindAll(agv => agv.agvcType == AGVC_TYPES.GangHau);
        /// <summary>
        /// 取得GPMAGV車
        /// </summary>
        public static List<IAGVC> GpmAGVList => AGVCList.FindAll(agv => agv.agvcType == AGVC_TYPES.GPM);

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


        /// <summary>
        /// 與所有的AGV車連線喔
        /// </summary>
        public static void ConnectToAGVCs()
        {
            AGVCInfoBinding();
            foreach (IAGVC agvc in AGVCList.FindAll(agvc => agvc.agvcInfos != null))
            {
                agvc.StateOnChanged += AgvcHandler.AgvcStateChangedHandle;
                agvc.ConnectToAGV();
            }
        }

        private static int AGVCInfoBinding()
        {
            int bindedAgvcNum = 0;
            foreach (var item in AGVSManager.CurrentAGVS.BindingAGVCInfoList)
            {
                var agvcSelected = AGVCList.FirstOrDefault(agvc => agvc.agvcInfos == null);
                if (agvcSelected != null)
                {
                    agvcSelected.agvcInfos = item;
                    bindedAgvcNum++;
                }
            }
            return bindedAgvcNum;
        }
    }
}
