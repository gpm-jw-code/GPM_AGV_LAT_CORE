using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl
{
    /// <summary>
    /// 交通管制
    /// </summary>
    internal class TrafficController
    {
        internal bool HasCrossPoint(List<string> path1, List<string> path2, out List<string> crossPts)
        {
            crossPts = new List<string>();
            foreach (var pt in path1)
            {
                if (path2.Contains(pt))
                    crossPts.Add(pt);
            }
            return crossPts.Count > 0;
        }


    }
}
