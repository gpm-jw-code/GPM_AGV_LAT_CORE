using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.GPMMiddleware.TrafficControl;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{
    internal class TaskOrderStateTrack
    {


        ILogger logger = new LoggerInstance(typeof(TaskOrderStateTrack));
        public IAGVS Agvs { get; }
        public IAGVC Agvc { get; }
        public clsHostExecuting NewExecuting { get; }

        internal TaskOrderStateTrack(IAGVS agvs, IAGVC agvc, clsHostExecuting newExecuting)
        {
            Agvs = agvs;
            Agvc = agvc;
            NewExecuting = newExecuting;
        }

        internal void StartTrack()
        {
            string task_id = NewExecuting.latOrderDetail.taskName;
            logger.InfoLog($"Order:{task_id}:Start State Tracking");
            Task.Run(async () =>
            {
                ORDER_STATE _preState = ORDER_STATE.NotFound;
                Agvc.agvcStates.MapStates.currentStationID = "Navigating";
               
                while (true)
                {
                    await Task.Delay(1000);
                    ORDER_STATE state = await Agvc.TaskStateDownload(task_id);
                    NewExecuting.State = state;

                    if (_preState != state)
                    {
                        logger.InfoLog($"Order:{task_id}:State Change , Current State={state}");
                        Agvs.agvsApi.ReportNagivateTaskState(NewExecuting);
                    }
                    _preState = state;
                    if (state == ORDER_STATE.COMPLETE)
                    {
                        break;
                    }
                }


                //Agvc.agvcStates.MapStates.currentStationID = NewExecuting.latOrderDetail.action.stationID;
                Agvc.agvcStates.MapStates.navigationState.pathStations.Clear();
                Agvc.agvcStates.MapStates.navigationState.IsNavigating = false;

                TrafficControlCenter.Remove(Agvc);

                logger.InfoLog($"Order:{task_id}:Mission Completed ! ");
            });
        }




    }
}
