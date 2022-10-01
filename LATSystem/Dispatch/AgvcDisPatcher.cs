using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPM_AGV_LAT_CORE.GPMMiddleware.AgvsHandler;

namespace GPM_AGV_LAT_CORE.LATSystem.Dispatch
{
    /// <summary>
    /// LAT派車系統
    /// </summary>
    public static class AgvcDisPatcher
    {

        public static void TaskDispatch(clsLATTaskOrder latOrder)
        {
            var agvc = AGVCManager.GetAGVCByEqName(latOrder.executEqName);
            if (agvc.agvcType == AGVC_TYPES.GangHau)
            {
                //
            }
        }



    }
}
