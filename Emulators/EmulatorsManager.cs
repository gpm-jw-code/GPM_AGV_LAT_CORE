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
        public static GangHaoAgvcEmulator[] gangHaoAgvcList = new GangHaoAgvcEmulator[2]
        {
             new GangHaoAgvcEmulator("192.168.0.111",19204),
             new GangHaoAgvcEmulator("192.168.0.233",19204),
        };

        internal static void Start()
        {
            kingGallentAgvc.Start();
            foreach (var gangHaoEmulator in gangHaoAgvcList)
            {
                gangHaoEmulator.Start();
            }
        }
    }
}
