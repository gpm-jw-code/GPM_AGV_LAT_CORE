using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public class clsOrderAGVSinfo
    {
        public clsOrderAGVSinfo(AGVS_TYPES agvsType)
        {
            this.agvsType = agvsType;
        }
        public AGVS_TYPES agvsType { get; private set; }
    }
}
