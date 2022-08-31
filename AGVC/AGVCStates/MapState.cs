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
