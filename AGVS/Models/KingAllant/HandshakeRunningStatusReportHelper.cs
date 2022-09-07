using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS.Models.KingAllant
{
    public class HandshakeRunningStatusReportHelper
    {
        public string Jsonstr { get; set; }
        private Dictionary<string, object> returnObj;

        public HandshakeRunningStatusReportHelper(AgvcInfoForKingAllant infos, int SystemBytes = 1)
        {
            returnObj = new Dictionary<string, object>()
            {
                    {"SID",infos.SID},
                    {"EQName",infos.EQName},
                    {"System Bytes",SystemBytes},
                    {"Header", new Dictionary<string,object>() },
            };
        }
        public HandshakeRunningStatusReportHelper(string SID, string EQName, int SystemBytes = 1)
        {
            returnObj = new Dictionary<string, object>()
            {
                    {"SID",SID},
                    {"EQName",EQName},
                    {"System Bytes",SystemBytes},
                    {"Header", new Dictionary<string,object>() },
            };
        }

        /// <summary>
        /// 建立一個模板 Caller去塞值
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, object> CreateStateReportDataModelTemplate()
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                {
                    "0105" ,new Dictionary<string, object>()
                    {
                        { "Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                        { "Coordination",null},
                        { "Last Visited Node",null},
                        { "AGV Status",null},
                        { "Escape Flag",null},
                        { "Sensor Status",null},
                        { "CPU Usage Percen",-1},
                        { "RAM Usage Percent",-1},
                        { "AGV Reset Flag",true},
                        { "Signal Strength",0},
                        { "Cargo Status",0},
                        { "CSTID",new string[0]},
                        { "Electric Volume",new double[2]{-1,-1 } },
                        { "Alarm Code", new List<object>()},
                        { "Fork Height", 0},
                    }
                }
            };
            return returnObj;
        }

        /// <summary>
        /// 建立一個模板 Caller去塞值
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, object> CreateStateReportDataModel(RunningStateReportModel model)
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                {
                    "0105" ,new Dictionary<string, object>()
                    {
                        { "Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                        { "Coordination",model.Corrdination},
                        { "Last Visited Node",model.LastVisitedNode},
                        { "AGV Status",model.AGVStatus},
                        { "Escape Flag",model.EscapeFlag},
                        { "Sensor Status",model.SensorStatus},
                        { "CPU Usage Percen",model.CpuUsagePercent},
                        { "RAM Usage Percent",model.RamUsagePercent},
                        { "AGV Reset Flag",model.AgvResetFlag},
                        { "Signal Strength",model.SignalStrength},
                        { "Cargo Status",model.CargoStatus},
                        { "CSTID",model.CSTID},
                        { "Electric Volume",model.ElectricVolume},
                        { "Alarm Code", model.AlarmCode},
                        { "Fork Height", model.ForkHeight},
                    }
                }
            };
            return returnObj;
        }

       


        /// <summary>
        /// 0101
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> CreateOnlineOfflineModeQuery()
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                { "0101" ,new Dictionary<string, object>(){{"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                } }
            };
            return returnObj;
        }


        /// <summary>
        /// 要求上線 0103
        /// </summary>
        /// <param name="ModeRequest">0:Offline;1:Online</param>
        /// <param name="CurrentNode">目前位於的QR Code 或 Tag名稱</param>
        /// <returns></returns>
        public Dictionary<string, object> CreateOnlineOfflineRequest(int ModeRequest, int CurrentNode)
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                { "0103" , new Dictionary<string, object>(){
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                    {"Mode Request",ModeRequest},
                    {"Current Node",CurrentNode},
                } }
            };
            return returnObj;
        }


        public Dictionary<string, object> CreateTaskDownload(string taskName)
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                { "0301" , new Dictionary<string, object>(){
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                    {"Task Name",taskName},
                } }
            };
            return returnObj;
        }

        /// <summary>
        /// Task Feedback
        /// </summary>
        /// <param name="ReturnCode"></param>
        /// <returns></returns>
        internal Dictionary<string, object> createTaskFeedback(string taskName, string taskSimplex, int taskSequence,
                                                                    int pointIndex, int taskStatus)
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                {"0303",new Dictionary<string, object>()
                {
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    {"Task Name",taskName },
                    {"Task Simplex",taskSimplex },
                    {"Task Sequence",taskSequence },
                    {"Point Index",pointIndex },
                    {"Task Status",taskStatus },
                } }
            };
            return returnObj;
        }
        internal Dictionary<string, object> CreateAGVSResetExcute(int resetMode)
        {
            returnObj["Header"] = new Dictionary<string, object>()
            {
                { "0305" , new Dictionary<string, object>(){
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss")},
                    {"Reset Mode",resetMode},
                } }
            };
            return returnObj;
        }
    }
}
