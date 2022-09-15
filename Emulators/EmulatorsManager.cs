using GPM_AGV_LAT_CORE.Emulators.GangHao;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators
{
    public static class EmulatorsManager
    {



        public static KingGallentAgvsEmulator kingGallentAgvc = new KingGallentAgvsEmulator("127.0.0.1", 5500);


        public static GangHaoAGVCEmulate[] gangHaoAgvcList = new GangHaoAGVCEmulate[2]
        {
             new GangHaoAGVCEmulate("192.168.0.104"),
             new GangHaoAGVCEmulate("192.168.0.233"),
        };

        internal static void Start()
        {
            kingGallentAgvc.Start();
            foreach (var gangHaoEmulator in gangHaoAgvcList)
            {
                gangHaoEmulator.Start();
            }
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
                mapEmulator = new GangHaoAgvc_MapEmulator(ip, 19206);
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
