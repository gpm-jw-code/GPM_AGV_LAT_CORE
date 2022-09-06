using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCInfo
{
    internal class AgvcInfoForGPM : IAgvcInfoToAgvs
    {
        public AGVS_TYPES agvs_type { get; set; } = AGVS_TYPES.GPM;
    }
}
