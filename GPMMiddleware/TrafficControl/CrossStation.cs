using GPM_AGV_LAT_CORE.AGVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.MapState;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl
{
    internal class CrossStation
    {
        internal CrossStation(string id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            Task.Run(() => Monitor());
        }

        private async Task Monitor()
        {
            while (true)
            {
                await Task.Delay(10);

                foreach (var item in nonPassedAgvcList)
                {
                    item.distance = Distance(item.agvc.agvcStates.MapStates.globalCoordinate);
                }

                closestAgvc = nonPassedAgvcList.OrderBy(x => x.distance).ToList().First();
                inControlRadiusAgvcList = nonPassedAgvcList.FindAll(a => a.distance < controlRadius);

                if (inControlRadiusAgvcList.Count > 1)
                {
                    var agvcsToPause = inControlRadiusAgvcList.FindAll(a => a.agvc != closestAgvc).Select(a => a.agvc).ToList();
                    //除了最接近交叉路口的AGVC以外，全部都給我暫停導航
                    PauseNavigate(agvcsToPause);
                    ResumeNavigate(agvcsToPause);
                    //等待最接近的那台通過之後 繼續導航
                }
            }
        }

        internal string id { get; set; }
        internal double x { get; set; }
        internal double y { get; set; }
        /// <summary>
        /// 管制半徑距離
        /// </summary>
        internal double controlRadius { get; set; } = 0.5;
        /// <summary>
        /// 已經過的AGV
        /// </summary>
        internal List<AGVCDistanceInfo> passedAgvcList { get; set; } = new List<AGVCDistanceInfo>();
        /// <summary>
        /// 尚未經過的AGV
        /// </summary>
        internal List<AGVCDistanceInfo> nonPassedAgvcList { get; set; } = new List<AGVCDistanceInfo>();

        internal List<AGVCDistanceInfo> inControlRadiusAgvcList { get; set; } = new List<AGVCDistanceInfo>();
        /// <summary>
        /// 距離該點最近的AGVC
        /// </summary>
        internal AGVCDistanceInfo closestAgvc;

        private ManualResetEvent closestAGVPassEvent;


        private void ResumeNavigate(List<IAGVC> agvcToPauseList)
        {

        }
        private void PauseNavigate(List<IAGVC> agvcToPauseList)
        {

        }

        private double Distance(GlobalCoordinate corrdinate)
        {
            return Math.Sqrt(Math.Pow((corrdinate.x - x), 2) + Math.Pow((corrdinate.y - y), 2));
        }


        internal class AGVCDistanceInfo
        {
            internal readonly IAGVC agvc;
            internal double distance;
            internal AGVCDistanceInfo(IAGVC aGVC)
            {
                this.agvc = aGVC;
            }
        }

    }
}
