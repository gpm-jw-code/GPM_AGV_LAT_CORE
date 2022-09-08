using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    public interface IAGVSExecutingState
    {
        dynamic executingObject { get; set; }
    }
}
