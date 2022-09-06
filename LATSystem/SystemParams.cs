using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.LATSystem
{
    public class SystemParams
    {
        /// <summary>
        /// 要使用的派車平台廠商
        /// </summary>
        public static AGVS_TYPES AgvsTypeToUse { get; set; } = AGVS_TYPES.KINGGALLENT;

        /// <summary>
        /// 要使用的車子廠商
        /// </summary>
        public static List<AGVC_TYPES> AvgcTypesToUse { get; set; } = new List<AGVC_TYPES>() { AGVC_TYPES.GangHau };

        public static string GangHaoRDSCoreServerUrl = "http://localhost:5279/api/Server";

    }
}
