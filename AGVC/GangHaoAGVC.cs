using GPM_AGV_LAT_CORE.LATSystem;
using System;
using GangHaoAGV.AGV;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCStates;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using System.Linq;
using System.Collections.Generic;
using GangHaoAGV.API;
using GPM_AGV_LAT_CORE.Logger;
using System.Deployment.Internal;
using GangHaoAGV.Models.StateModels.Responses;
using static GangHaoAGV.Models.StateModels.Responses.robotStatusStationRes_11301;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.MapState;
using static GPM_AGV_LAT_CORE.AGVC.AGVCStates.AlarmStates;

namespace GPM_AGV_LAT_CORE.AGVC
{
    public class GangHaoAGVC : AGVCBase
    {

        public GangHaoAGVC()
        {
            agvcType = AGVC_TYPES.GangHau;
            logger = new LoggerInstance(GetType());
        }

        /// <summary>
        /// AGVC控制實例
        /// </summary>
        public cAGV AGVInterface { get; set; }

        protected override bool ConnectoAGVInstance()
        {
            AGVInterface = new cAGV(agvcParameters.tcpParams.HostIP);
            AGVInterface.NAVIGATIOR.OnReachPoint += NAVIGATIOR_OnReachPoint;
            return true;
        }

        public override async Task<bool> RelocProcess()
        {
            bool reloc_finish = await AGVInterface.CONTROL.Reloc();
            if (reloc_finish)
            {
                await Task.Delay(1000);
                logger.WarnLog($"地圖信心度:{AGVInterface.STATES.locationInfo.confidence}");
                if (AGVInterface.STATES.locationInfo.confidence < 0.5)
                {
                    logger.WarnLog($"重定位失敗:信心度過低(<0.5)");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
                return false;
        }
        protected override async Task<MapInfo> LoadMapStationStored()
        {
            var mapName = AGVInterface.STATES.mapLoadInfo.current_map;
            var latStations = AGVInterface.STATES.stationLoadInfo.stations.Select(st => new StationInfo()
            {
                desc = st.desc,
                id = st.id,
                type = st.type,
                x = st.x,
                y = st.y,
                r = st.r,
            }).ToList();
            return new MapInfo()
            {
                name = mapName,
                stations = latStations
            };
        }

        public override List<string> GetMapNames()
        {
            try
            {
                if (AGVInterface == null)
                    return new List<string>();
                List<string> mapNames = AGVInterface.STATES.mapLoadInfo.maps.ToList();
                return mapNames;
            }
            catch (Exception ex)
            {
                logger.WarnLog($"{EQName} Get Map Names fail:{ex.Message}");
                return new List<string>();
            }
        }

        private void NAVIGATIOR_OnReachPoint(object sender, GangHaoAGV.Models.MapModels.Requests.robotMapTaskGoTargetReq_3051 taskInfo)
        {
        }

        protected override async Task SyncStateInstance()
        {
            while (!AGVInterface.StatesPortConnected | AGVInterface.STATES == null)
            {
                agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTING;
                await Task.Delay(TimeSpan.FromSeconds(1));
                logger.TraceLog("等待罡豪State Port 連線...");
            }
            if (!AGVInterface.STATES.stateFetching)
            {
                AGVInterface.STATES.StartStatesDataSync();
                await Task.Delay(1000);
            }

            agvcStates.States.EConnectionState = CONNECTION_STATE.CONNECTED;
            agvcStates.BetteryState.remaining = AGVInterface.STATES.betteryState.battery_level;
            agvcStates.BetteryState.charging = AGVInterface.STATES.betteryState.charging;
            agvcStates.MapStates.globalCoordinate.x = AGVInterface.STATES.locationInfo.x;
            agvcStates.MapStates.globalCoordinate.y = AGVInterface.STATES.locationInfo.y;
            agvcStates.MapStates.globalCoordinate.r = AGVInterface.STATES.locationInfo.angle;

            agvcStates.MapStates.currentStationID = AGVInterface.STATES.locationInfo.current_station == "" ? agvcStates.MapStates.currentStationID : AGVInterface.STATES.locationInfo.current_station;
            agvcStates.MapStates.currentMapInfo.name = AGVInterface.STATES.mapLoadInfo.current_map;
            agvcStates.MapStates.currentMapInfo.stations = AGVInterface.STATES.stationLoadInfo.stations.Select(st => new StationInfo()
            {
                desc = st.desc,
                id = st.id,
                r = st.r,
                x = st.x,
                y = st.y,
                type = st.type,

            }).ToList();

            robotStatusAlarmRes_11050 alarms = AGVInterface.STATES.alarms;
            agvcStates.AlarmState.Update(GetLatAlarm(alarms));
            base.SyncStateInstance();
        }

        protected override Task SyncOrderStateInstance()
        {
            return base.SyncOrderStateInstance();
        }

        public override AlarmStates GetLatAlarm(object nativeAlarm)
        {
            robotStatusAlarmRes_11050 alarms = (robotStatusAlarmRes_11050)nativeAlarm;

            List<Alarm> fatals = alarms.fatals.Length == 0 ? null : GangAlarmParse(alarms.fatals);

            List<Alarm> errors = alarms.errors.Length == 0 ? null : GangAlarmParse(alarms.errors);

            List<Alarm> warnings = alarms.warnings.Length == 0 ? null : GangAlarmParse(alarms.warnings);

            List<Alarm> notices = alarms.notices.Length == 0 ? null : GangAlarmParse(alarms.notices);


            return new AlarmStates()
            {
                Fatals = fatals,
                Warnings = warnings,
                Notices = notices,
                Errors = errors
            };
        }

        private List<Alarm> GangAlarmParse(Dictionary<string, object>[] ganAlarms)
        {
            List<Alarm> alarmList = new List<Alarm>();
            foreach (var item in ganAlarms)
            {
                string code = item.Keys.FirstOrDefault(key => int.TryParse(key, out int _));
                if (code == null)
                    continue;
                int timestamp = int.Parse(item[code].ToString());
                if (timestamp <= 0)
                    continue;
                DateTime time = new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(timestamp);
                string description = item["desc"].ToString();
                alarmList.Add(new Alarm()
                {
                    code = code,
                    time = time,
                    description = description
                });
            };
            return alarmList;
        }
        protected override async Task SyncSyncOrderExecuteStateInstance()
        {
            await Task.Run(async () =>
            {
                List<string> orderIDList = orderList_LAT.Select(order => order.latOrderDetail.taskName).ToList();
                foreach (var order in orderList_LAT)
                {
                    var orderID = order.latOrderDetail.taskName;
                    //TODO 
                    // GangHaoAGV.Models.Order.OrderDetails orderState = await RDSCoreServer.QueryOrderState(orderID);
                    //order.State = GetLatOrderStateByGangOrderState(orderState.state);
                }
            });
            await base.SyncSyncOrderExecuteStateInstance();
        }

        public override async Task<ORDER_STATE> TaskStateDownload(string taskName)
        {
            AGVInterface.STATES.currentTaskid = taskName;

            var taskStatusPackage = AGVInterface.STATES.taskStatusPakage.task_status_package;
            var task_status_list = taskStatusPackage.task_status_list;
            //var req = await AGVInterface.STATES.API.GetTaskStatusPackage(new string[] { taskName });
            if (task_status_list == null)
                return ORDER_STATE.WAIT_EXECUTE;

            var taskStatus = task_status_list.FirstOrDefault(task => task.task_id == taskName);
            if (taskStatus != null)
            {
                logger.TraceLog($"任務{taskName} |狀態 = {taskStatus.status}");
                return GangHaoStatusToLATStates(taskStatus.status);
            }
            else
                return ORDER_STATE.WAIT_EXECUTE;
        }

        public override async Task<object> GetNativeAlarmState()
        {
            return AGVInterface.STATES.alarms;
        }

        public override async Task PauseNavigate()
        {
            await AGVInterface.NAVIGATIOR.PauseNavigate();
        }
        public override async Task ResumeNavigate()
        {
            await AGVInterface.NAVIGATIOR.ResumeNavigate();
        }
        private ORDER_STATE GangHaoStatusToLATStates(int gangHaoStatus)
        {
            switch (gangHaoStatus)
            {
                case 0:
                    return ORDER_STATE.StatusNone;
                case 1:
                    return ORDER_STATE.WAIT_EXECUTE;
                case 2:
                    return ORDER_STATE.EXECUTING;
                case 3:
                    return ORDER_STATE.SUSPEND;
                case 4:
                    return ORDER_STATE.COMPLETE;
                case 5:
                    return ORDER_STATE.FAILED;
                case 6:
                    return ORDER_STATE.CANCELED;
                case 7:
                    return ORDER_STATE.OverTime;
                case 404:
                    return ORDER_STATE.NotFound;
                default:
                    return ORDER_STATE.NotFound;
            }
        }


        private ORDER_STATE GetLatOrderStateByGangOrderState(string gangHaoTaskState)
        {
            switch (gangHaoTaskState)
            {
                case "CREATED":
                    return ORDER_STATE.EXECUTING;
                case "TOBEDISPATCHED":
                    return ORDER_STATE.WAIT_EXECUTE;
                case "RUNNING":
                    return ORDER_STATE.EXECUTING;
                case "FINISHED":
                    return ORDER_STATE.COMPLETE;
                case "FAILED":
                    return ORDER_STATE.FAILED;
                case "STOPPED":
                    return ORDER_STATE.STOPPED;
                case "Error":
                    return ORDER_STATE.ERROR;
                case "WAITING":
                    return ORDER_STATE.WAIT_EXECUTE;
                default:
                    return ORDER_STATE.WAIT_EXECUTE;
            }
        }
    }
}
