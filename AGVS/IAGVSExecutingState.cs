using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    public interface IAGVSExecutingState
    {
        ORDER_STATE state { get; set; }
        dynamic executingObject { get; set; }
    }
}
