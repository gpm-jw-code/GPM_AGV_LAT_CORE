using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCStates
{
    public class MapState
    {

        public GlobalCoordinate globalCoordinate { get; internal set; } = new GlobalCoordinate();
        public RobotCoordinate robotCorrdinate { get; internal set; } = new RobotCoordinate();
        public MapInfo currentMapInfo { get; set; } = new MapInfo();
        public NavigationState navigationState { get; set; } = new NavigationState();
        public string currentStationID { get; internal set; } = "Unknown";

        public class NavigationState
        {
            public bool IsNavigating { get; set; } = false;
            public string targetStationID { get; set; } = "";
            public string nextStationID { get; set; }
            /// <summary>
            /// 沿途會經過的所有站點
            /// </summary>
            public List<string> pathStations { get; set; } = new List<string>();
        }

        public class MapInfo
        {
            public string name { get; set; }
            public string mapFileUrl { get; set; }
            public List<StationInfo> stations { get; set; } = new List<StationInfo>();
        }

        public class StationInfo : GlobalCoordinate
        {
            public string id { get; set; }
            public string desc { get; set; }
            public string type { get; set; }
        }

        /// <summary>
        /// 世界座標系
        /// </summary>
        public class GlobalCoordinate
        {
            public double x { get; set; } = 0;
            public double y { get; set; } = 0;
            public double r { get; set; } = 0;
        }

        /// <summary>
        /// 機器人坐標系
        /// </summary>
        public class RobotCoordinate
        {

            /// <summary>
            /// X軸向速度
            /// </summary>
            public double vx { get; internal set; } = 0;
            /// <summary>
            /// Y軸向速度
            /// </summary>
            public double vy { get; internal set; } = 0;
            /// <summary>
            /// 角速度
            /// </summary>
            public double w { get; internal set; } = 0;
        }
    }
}
