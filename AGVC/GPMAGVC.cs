using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Logger;
using GPM_AGV_LAT_CORE.Parameters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class GPMAGVC : AGVCBase
    {

        public GPMAGVC()
        {
            agvcType = AGVC_TYPES.GPM;
            logger = new LoggerInstance(GetType());
        }

        protected override bool ConnectoAGVInstance()
        {
            Console.WriteLine("GPM AGVC Connect simulation..return true");
            return true;
        }

        protected override async Task SyncStateInstance()
        {
        }
        protected override Task SyncOrderStateInstance()
        {
            return base.SyncOrderStateInstance();
        }


        protected override Task SyncSyncOrderExecuteStateInstance()
        {
            return base.SyncSyncOrderExecuteStateInstance();
        }


    }
}
