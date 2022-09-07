using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public class clsAGVCReset
    {
        public RESET_MODE ResetMode { get; set; }
        public enum RESET_MODE
        {
            Abort, CycleStop
        }
    }
}
