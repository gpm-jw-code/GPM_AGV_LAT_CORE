using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCStates
{
    /// <summary>
    /// 電池狀態
    /// </summary>
    public class BetteryStates
    {
        public double remaining { get; internal set; } = -1;
        public bool charging { get; internal set; } = false;
    }
}
