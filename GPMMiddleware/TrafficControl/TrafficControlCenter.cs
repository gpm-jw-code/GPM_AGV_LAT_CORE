using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl
{
    /// <summary>
    /// 交通管制
    /// </summary>
    public class TrafficControlCenter
    {
        public static bool Enable { get; set; } = false;
        /// <summary>
        /// 站點資訊
        /// </summary>
        public static Dictionary<string, Station> Stations { get; set; } = new Dictionary<string, Station>()
        {
            {"LM1", new Station("LM1",new Point(1,1.5,0)){ } },
            {"LM2", new Station("LM2",new Point(1,0.5,0)){ } },
            {"LM3", new Station("LM3",new Point(1.5,0.0,0)){ } },
            {"LM4", new Station("LM4",new Point(1,-0.5,0)){ } },
            {"LM5", new Station("LM5",new Point(1.5,-1,0)){ } },
            {"VP1", new Station("VP1",new Point(0,0.5,0)){ } },
            {"VP2", new Station("VP2",new Point(0.7,0.5,0)){ } },
            {"CP1", new Station("CP1",new Point(-1,0.5,0)){ } },
            {"CP2", new Station("CP2",new Point(1.5,-1.6,0)){ } },
        };


        public static Dictionary<string, CrossStation> ControlingStation = new Dictionary<string, CrossStation>();


        public static void JoinTrafficSystem(IAGVC agv, List<string> path) //TODO 
        {

            if (!Enable)
                return;

            foreach (var station in path)
            {
                if (!ControlingStation.ContainsKey(station))
                {
                    Station sinfo = Stations[station];
                    var crossStation = new CrossStation(station, sinfo.targetPt.x, sinfo.targetPt.y);
                    crossStation.StartMonitor();
                    ControlingStation.Add(station, crossStation);

                }

                ControlingStation[station].AddAGVC(agv);
            }

        }

        internal static void Remove(IAGVC agvc)
        {

            foreach (var item in ControlingStation)
            {
                if (item.Value.inControlingAgvcList.ContainsKey(agvc))
                {
                    item.Value.inControlingAgvcList.Remove(agvc);
                }


            }
            foreach (var item in ControlingStation.ToList())
            {
                if (item.Value.inControlingAgvcList.Count == 0)
                {
                    ControlingStation.Remove(item.Key);
                }
            }

        }

        public static void TrafficControlStationsInitialze()
        {
            var crossStations = GetCrossPaths();
            foreach (string stationID in crossStations)
            {
                var agvcList = AGVCManager.FindAGVCByCurrentPathIncludeStation(stationID);
                if (ControlingStation.ContainsKey(stationID))
                {
                    var crossStation = ControlingStation[stationID];
                    crossStation.AddAGVCs(agvcList);
                }
                else
                {
                    Station sinfo = Stations[stationID];
                    var crossStation = new CrossStation(stationID, sinfo.targetPt.x, sinfo.targetPt.y);
                    crossStation.AddAGVCs(agvcList);

                    ControlingStation.Add(stationID, crossStation);
                    crossStation.StartMonitor();
                }
            }
        }




        public static List<string> GetCrossPaths(IAGVC agv)
        {
            List<string> pathToRun = agv.agvcStates.MapStates.navigationState.pathStations;
            List<IAGVC> agvclist = AGVCManager.GetNavigatingAGVCs();

            agvclist = agvclist.FindAll(_agv => _agv.EQName != agv.EQName);

            List<string> overlapStationList = new List<string>();
            foreach (IAGVC _agv in agvclist)
            {
                List<string> _path = _agv.agvcStates.MapStates.navigationState.pathStations;
                List<string> _overlapStationList = _path.FindAll(station => pathToRun.Contains(station));
                overlapStationList.AddRange(_overlapStationList);
            }

            overlapStationList = overlapStationList.Distinct().ToList();
            return overlapStationList;
        }


        public static List<string> GetCrossPaths()
        {

            List<clsHostExecuting> orders = OrderManerger.OrderList.FindAll(order => order.State != Manergers.Order.ORDER_STATE.COMPLETE);
            IEnumerable<List<string>> pathsList = orders.Select(or => or.latOrderDetail.action.paths).ToList();


            Dictionary<string, int> repeat = new Dictionary<string, int>();
            foreach (List<string> path in pathsList)
            {
                foreach (var station in path)
                {
                    if (repeat.ContainsKey(station))
                        repeat[station]++;
                    else
                        repeat.Add(station, 1);
                }
            }

            List<string> repeatedStations = repeat.ToList().FindAll(kp => kp.Value > 1).Select(kp => kp.Key).ToList();
            return repeatedStations;
        }

    }

    public class Station
    {

        public Station()
        {

        }

        public Station(string stationID, Point targetPt)
        {
            this.stationID = stationID;
            this.targetPt = targetPt;
        }
        public string stationID { get; set; }
        public Point targetPt { get; set; }
        /// <summary>
        /// 沿途會經過\停留的站點
        /// </summary>
        public List<Station> pathStations { get; set; } = new List<Station>();
    }
    public class Point
    {
        public Point(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
}
