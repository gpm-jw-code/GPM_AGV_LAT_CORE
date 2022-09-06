using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCInfo
{
    /// <summary>
    /// AGVS看得懂的AGVC資訊
    /// </summary>
    public interface IAgvcInfoToAgvs
    {
        AGVS_TYPES agvs_type { get; set; }
    }
}
