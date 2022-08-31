using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCInfo
{
    public class AgvcInfoForKingAllant : IAgvcInfoToAgvs
    {
        public string AGVID { get; set; } = "001";
        public string StationID { get; set; } = "001";
        public string EQID { get; set; } = "001";
        public string SID => string.Join(":", new string[] { AGVID, StationID, EQID });
        public string EQName { get; set; } = "AGV_001";
    }
}
