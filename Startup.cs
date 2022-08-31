using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangHaoAGV;
using GPM_AGV_LAT_CORE.Emulators;

namespace GPM_AGV_LAT_CORE
{
    public class Startup
    {

        public Startup()
        {

        }

        public static GangHaoAGV.AGV.cAGV agv1;
        public static GangHaoAGV.AGV.cAGV agv2;
        public static async Task StartService()
        {
            await Task.Delay(1).ContinueWith(tsk =>
            {
                Task.Run(() => EmulatorsManager.Start());
                //TODO Read AGVS 
                AGVS.AGVSManager.ConnectToHosts();
                //TODO Read AGVC 
                AGVC.AGVCManager.ConnectToAGVCs();
            });
        }
    }
}
