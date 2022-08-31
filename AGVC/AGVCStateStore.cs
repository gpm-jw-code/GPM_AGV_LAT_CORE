using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC
{
    /// <summary>
    /// 車子狀態儲存
    /// </summary>
    public class AGVCStateStore
    {
        /// <summary>
        /// 是否已連線
        /// </summary>
        public bool Connected { get; set; } = false;

        /// <summary>
        /// IDLE/RUNNING; MANUAL/IDLE ; ONLINE/OFFLINE 等狀態
        /// </summary>
        public AGVCRUNStates States { get; set; } = new AGVCRUNStates();

        /// <summary>
        /// 坐標系狀態，包含世界坐標系與機器人坐標系
        /// </summary>
        public MapState MapStates { get; set; } = new MapState();

        /// <summary>
        /// 電池相關狀態
        /// </summary>
        public BetteryStates BetteryState { get; set; } = new BetteryStates();

    }
}
