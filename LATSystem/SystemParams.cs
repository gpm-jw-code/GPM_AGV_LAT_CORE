using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.LATSystem
{
    public class SystemParams
    {

        public static bool IsAGVS_Simulation { get; set; } = false;

        /// <summary>
        /// 要使用的派車平台廠商
        /// </summary>
        public static AGVS_TYPES AgvsTypeToUse { get; set; } = AGVS_TYPES.KINGGALLENT;

        /// <summary>
        /// 要使用的車子廠商
        /// </summary>
        public static List<AGVC_TYPES> AvgcTypesToUse { get; set; } = new List<AGVC_TYPES>() { AGVC_TYPES.GangHau };

        public static string GangHaoRDSCoreServerUrl { get; set; } = "http://localhost:5279/api/Server";
        public static string KingGallentAGVSEmulatorServerUrl { get; set; } = "http://localhost:5000/api/KingGallentAGVSEmulator";



    }
}
