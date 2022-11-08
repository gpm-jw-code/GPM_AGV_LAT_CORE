using System.Collections.Generic;

namespace GPM_AGV_LAT_CORE.Emulators.KingGallentAGVS
{

    public partial class KingGallentAgvsEmulator
    {
        public class OrderTask
        {
            public OrderTask()
            {

            }
            public OrderTask(string TaskID, List<string> stationIDs)
            {
                this.TaskID = TaskID;
                Stations = stationIDs;
                foreach (var stationID in stationIDs)
                {
                    var stationInfo = new StationInfo() { stationID = stationID, Status = 0 };
                    StationsQueue.Enqueue(stationInfo);
                }
            }

            public string TaskID { get; set; }

            internal Queue<StationInfo> StationsQueue { get; set; } = new Queue<StationInfo>();
            public List<string> Stations { get; set; } = new List<string>();

            public List<string> ReachedStationIDList { get; private set; } = new List<string>();

            public void StationReachReport(string stationID)
            {
                ReachedStationIDList.Add(stationID);
            }

            public class StationInfo
            {
                public string stationID { get; set; }
                internal int Status { get; set; }
            }
        }


    }
}
