using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.Emulators.GangHao;
using GPM_AGV_LAT_CORE.Emulators.KingGallentAGVS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators
{

    public class EmulationSetting
    {
        public AgvsParam AGVS { get; set; }
        public List<AgvcParam> AGVCList { get; set; }

    }

    public class AgvcParam
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string ROS_BOT_URI { get; set; }
    }

    public class AgvsParam
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }


    public static partial class EmulatorsManager
    {
        public struct AGVSSettings
        {
            public static string IP = "127.0.0.1";
            public static int Port = 5500;
        }

        public struct AGVCSettings
        {
            public static List<AgvcParam> AgvcList { get; set; }
        }

        public static KingGallentAgvsEmulator kingGallentAgvc = new KingGallentAgvsEmulator(AGVSSettings.IP, AGVSSettings.Port);
        public static List<GangHaoAGVCEmulate> gangHaoAgvcList = new List<GangHaoAGVCEmulate>()
        {
        };

        /// <summary>
        /// 僅啟動agvc模擬
        /// </summary>
        public static void StartAGVCEmuOnly()
        {
            gangHaoAgvcList = AGVCSettings.AgvcList.Select(acp => new GangHaoAGVCEmulate(acp.IP)).ToList();
            foreach (var gangHaoEmulator in gangHaoAgvcList)
                gangHaoEmulator.Start();
        }
        /// <summary>
        /// 僅啟動agcs模擬
        /// </summary>
        public static void StartAGVSEmuOnly()
        {
            kingGallentAgvc.Start();
        }

        public static void Start()
        {
            StartAGVCEmuOnly();
            StartAGVSEmuOnly();
        }
    }
}
