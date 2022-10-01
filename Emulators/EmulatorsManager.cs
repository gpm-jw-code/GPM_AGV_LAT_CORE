using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.Emulators.GangHao;
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


    public static class EmulatorsManager
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

        /// <summary>
        /// 罡豪AGVC通訊Port響應模擬
        /// </summary>
        public class GangHaoAGVCEmulate
        {
            /// <summary>
            /// 模擬車IP
            /// </summary>
            public string IP { get; private set; }

            /// <summary>
            /// 罡豪AGVC通訊Port響應模擬
            /// </summary>
            /// <param name="ip">車子IP</param>
            public GangHaoAGVCEmulate(string ip)
            {
                IP = ip;
                stateEmulator = new GangHaoAgvc_StateEmulator(ip, 19204);
                controlEmulator = new GangHaoAgvc_ControlEmulator(ip, 19205);
                mapEmulator = new GangHaoAgvc_MapEmulator(ip, 19206, stateEmulator);
            }
            /// <summary>
            /// 狀態模擬
            /// </summary>
            public GangHaoAgvc_StateEmulator stateEmulator { get; set; }
            /// <summary>
            /// 控制模擬
            /// </summary>
            public GangHaoAgvc_ControlEmulator controlEmulator { get; set; }
            /// <summary>
            /// 導航模擬
            /// </summary>
            public GangHaoAgvc_MapEmulator mapEmulator { get; set; }

            /// <summary>
            /// 把各個Server打開 開始監聽連線與訊息
            /// </summary>
            public void Start()
            {
                stateEmulator.Start();
                controlEmulator.Start();
                mapEmulator.Start();
            }

        }
    }
}
