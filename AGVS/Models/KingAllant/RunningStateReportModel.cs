using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS.Models.KingAllant
{
    public class RunningStateReportModel
    {
        public clsCorrdination Corrdination { get; set; } = new clsCorrdination();
        public int LastVisitedNode { get; set; }
        public int AGVStatus { get; set; } = 1;
        public bool EscapeFlag { get; set; } = false;

        public object SensorStatus { get; set; }

        public double CpuUsagePercent { get; set; } = 0;
        public double RamUsagePercent { get; set; } = 0;
        public bool AgvResetFlag { get; set; } = false;
        public double SignalStrength { get; set; } = 0;
        public int CargoStatus { get; set; } = 0;
        public string[] CSTID { get; set; } = new string[0];
        /// <summary>
        /// 里程數
        /// </summary>
        public double Odometry { get; set; } = 0;
        public double[] ElectricVolume { get; set; } = new double[2];

        public clsAlarmCode[] AlarmCode { get; set; } = new clsAlarmCode[0];
        public int ForkHeight { get; set; } = 0;

        public class clsCorrdination
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Theta { get; set; }
        }

        public class clsAlarmCode
        {
            public int AlarmID { get; set; }
            public int AlarmLevel { get; set; }
            public int AlarmCategory { get; set; }
            public string AlarmDescription { get; set; }
        }
    }
}
