using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.MapState;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl
{
    public class CrossStation
    {

        ILogger logger;

        public string id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        /// <summary>
        /// 管制半徑距離
        /// </summary>
        public double controlRadius { get; set; } = 0.5;
        /// <summary>
        /// 已經過的AGV
        /// </summary>
        public List<AGVCDistanceInfo> passedAgvcList
        {
            get
            {
                return inControlingAgvcList.Values.ToList().FindAll(s => s.pose_state == AGVCDistanceInfo.POSE_STATE.AWAY);
            }
        }
        /// <summary>
        /// 尚未經過的AGV
        /// </summary>
        public List<AGVCDistanceInfo> nonPassedAgvcList
        {
            get
            {
                return inControlingAgvcList.Values.ToList().FindAll(s => s.pose_state != AGVCDistanceInfo.POSE_STATE.AWAY);
            }
        }

        public List<AGVCDistanceInfo> inControlRadiusAgvcList
        {
            get
            {
                return inControlingAgvcList.Values.ToList().FindAll(s => s.distance <= controlRadius);

            }
        }

        public List<AGVCDistanceInfo> inControlingAgvcStateList => inControlingAgvcList.Values.ToList();

        internal Dictionary<IAGVC, AGVCDistanceInfo> inControlingAgvcList { get; set; } = new Dictionary<IAGVC, AGVCDistanceInfo>();
        /// <summary>
        /// 距離該點最近的AGVC
        /// </summary>
        public AGVCDistanceInfo closestAgvc
        {
            get
            {
                if (inControlRadiusAgvcList.Count == 0)
                    return null;
                return inControlRadiusAgvcList.OrderBy(s => s.distance).First();
            }
        }

        private ManualResetEvent closestAGVPassEvent;


        public CrossStation(string id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            logger = new LoggerInstance($"{typeof(CrossStation)}-{id}");
        }



        public void StartMonitor()
        {
            Task.Run(() => DistanceUpdating());
            Task.Run(() => ControlTraffic());
        }

        private async Task ControlTraffic()
        {
            while (true)
            {
                await Task.Delay(1);

                if (inControlingAgvcList.Count == 0)
                    break;

                try
                {
                    if (inControlRadiusAgvcList.Count < 2)
                    {
                        continue;
                    }

                    while (inControlRadiusAgvcList.Count > 0)
                    {
                        await Task.Delay(100);
                        try
                        {
                            var _closestAgvc = closestAgvc;

                            PauseNavigate(inControlRadiusAgvcList.FindAll(a => a.agvc_name != _closestAgvc.agvc_name).Select(a => a.agvc).ToList());
                            ///最接近站點的通過
                            ResumeNavigate(new List<IAGVC> { _closestAgvc.agvc });

                            ///等待通過
                            while (_closestAgvc.distance <= controlRadius) //還在管制區=>內等待直到脫離管制區
                            {
                                await Task.Delay(10);
                            }
                            logger.TraceLog($"AGVC({_closestAgvc.agvc.EQName}) far away control region");
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task DistanceUpdating()
        {
            while (true)
            {
                await Task.Delay(10);
                UpdatingAGVCDistance();
                if (inControlingAgvcList.Count == 0)
                    break;
            }
        }

        private void UpdatingAGVCDistance()
        {
            foreach (KeyValuePair<IAGVC, AGVCDistanceInfo> item in inControlingAgvcList)
            {
                item.Value.distance = Distance(item.Key.agvcStates.MapStates.globalCoordinate);
            }
        }

        private async void ResumeNavigate(List<IAGVC> agvcToPauseList)
        {

            try
            {
                logger.TraceLog($"Resume {agvcToPauseList.Count} AGVC Navigate.({string.Join(",", agvcToPauseList.Select(a => a.EQName))})");
                foreach (var agvc in agvcToPauseList)
                {
                    if (agvc != null)
                        await agvc?.ResumeNavigate();
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async void PauseNavigate(List<IAGVC> agvcToPauseList)
        {
            logger.TraceLog($"Pause {agvcToPauseList.Count} AGVC Navigate.({string.Join(",", agvcToPauseList.Select(a => a.EQName))})");

            foreach (var agvc in agvcToPauseList)
            {
                await agvc.PauseNavigate();
            }
        }

        private double Distance(GlobalCoordinate corrdinate)
        {
            return Math.Sqrt(Math.Pow((corrdinate.x - x), 2) + Math.Pow((corrdinate.y - y), 2));
        }
        internal void AddAGVC(IAGVC agv)
        {
            if (!inControlingAgvcList.ContainsKey(agv))
                inControlingAgvcList.Add(agv, new AGVCDistanceInfo(agv));
            else
                inControlingAgvcList[agv].agvc = agv;



        }
        internal void AddAGVCs(List<IAGVC> agvcList)
        {
            foreach (var agvc in agvcList)
            {
                if (!inControlingAgvcList.ContainsKey(agvc))
                    inControlingAgvcList.Add(agvc, new AGVCDistanceInfo(agvc));
                else
                    inControlingAgvcList[agvc].agvc = agvc;
            }
        }

        public class AGVCDistanceInfo
        {
            public enum POSE_STATE
            {
                /// <summary>
                /// 停止
                /// </summary>
                STOP,
                /// <summary>
                /// 遠離
                /// </summary>
                AWAY,
                /// <summary>
                /// 接近
                /// </summary>
                CLOSE_TO
            }
            internal IAGVC agvc;
            public string agvc_name => agvc.EQName;
            private double _distance = -1;

            public POSE_STATE pose_state { get; private set; } = POSE_STATE.STOP;
            public double distance
            {
                get => _distance;
                set
                {

                    if (value == _distance)
                        pose_state = POSE_STATE.STOP;
                    else if (value < _distance)
                        pose_state = POSE_STATE.CLOSE_TO;
                    else
                        pose_state = POSE_STATE.AWAY;

                    _distance = value;
                }
            }

            public AGVCDistanceInfo(IAGVC aGVC)
            {
                this.agvc = aGVC;
            }
        }

    }
}
