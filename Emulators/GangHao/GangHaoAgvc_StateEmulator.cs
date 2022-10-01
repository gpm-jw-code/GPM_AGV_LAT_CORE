using GangHaoAGV.Models;
using GangHaoAGV.Models.StateModels.Responses;
using GangHaoAGV.Models.ControlModels.Responses;
using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GangHaoAGV.Models.StateModels.Responses.robotStatusRelocRes_11021;
using System.Net.Sockets;
using GPM_AGV_LAT_CORE.Logger;
using System.Threading;
using GangHaoAGV.Models.MapModels.Requests;

namespace GPM_AGV_LAT_CORE.Emulators.GangHao
{
    public class GangHaoAgvc_StateEmulator : KingGallentAgvsEmulator
    {

        /// <summary>
        /// 報文數據結構
        /// </summary>
        public class ProtocolData
        {

            public int jsonDataLength { get; set; }
            /// <summary>
            /// 報文類型
            /// </summary>
            public int cmdType { get; set; }

            public int replyCmdType { get => cmdType + 10000; }

            public dynamic jsonData { get; set; }
        }

        private int LocationXInitial = 0;
        private int LocationYInitial = 0;

        public RELOC_STATE relocState = RELOC_STATE.RELOCING;
        public Dictionary<Dictionary<string, string>, bool> MapStatus = new Dictionary<Dictionary<string, string>, bool>();

        double[] currentPosition = new double[3] { 0, 0, 0 };
        public Dictionary<string, int> taskStatus = new Dictionary<string, int>();

        public string ros_bot_uri { get; set; }
        public bool isNavigating
        {
            get
            {
                return NavigateState.task_status == 2;
            }
        }

        public void ClearAlarm()
        {
            AgvcAlarm.Clear();
        }

        private bool isMapFinish
        {
            get
            {
                if (MapStatus.Count == 0) return false;
                bool isFinish = MapStatus.Values.Last();
                return isFinish;
            }
        }

        public GangHaoAgvc_StateEmulator(string ip, int port) : base(ip, port)
        {
            LocationXInitial = new Random().Next(1, 3);
            LocationYInitial = new Random().Next(-3, -1);
            logger = new LoggerInstance(typeof(GangHaoAgvc_StateEmulator));
            RosRobotStateSync();
        }


        robotStatusTaskRes_11020 NavigateState = new robotStatusTaskRes_11020();
        private void RosRobotStateSync()
        {
            Task.Run(async () =>
            {
                ROSEmu.ROSCoreAPI rosAPI = new ROSEmu.ROSCoreAPI();

                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(300));
                    robotStatusTaskRes_11020 _NavigateState = await rosAPI.GetNavigatingState();
                    NavigateState = _NavigateState;
                }

            });
            Task.Run(async () =>
          {
              ROSEmu.ROSCoreAPI rosAPI = new ROSEmu.ROSCoreAPI();

              while (true)
              {
                  await Task.Delay(TimeSpan.FromMilliseconds(300));
                  currentPosition = await rosAPI.GetCurrentPosition();
              }

          });
            Task.Run(async () =>
          {
              ROSEmu.ROSCoreAPI rosAPI = new ROSEmu.ROSCoreAPI();

              while (true)
              {
                  await Task.Delay(TimeSpan.FromMilliseconds(300));
                  taskStatus = await rosAPI.GetTaskStatus();
              }

          });
        }

        internal void AddNewMapReqStates(dynamic jsonData)
        {
            var reqObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
            MapStatus.Add(reqObj, false);
        }


        public override void Start()
        {
            Task.Factory.StartNew(() =>
            {
                server.OnMessageReceive += Server_OnClientMessageRev;
                server.OnClientConnected += Server_OnClientConnected;
                server.Listen();
            });
        }
        protected override void Server_OnClientConnected(object sender, Socket socket)
        {
            //
        }
        ProtocolData ProtocolDataParse(byte[] revBytes)
        {
            byte[] cmdTypeNoBytes = new byte[2] { revBytes[9], revBytes[8] };
            int cmbTypeNo = BitConverter.ToInt16(cmdTypeNoBytes, 0);
            byte[] oriByt = revBytes.ToArray();
            byte[] returnHead = new ArraySegment<byte>(oriByt, 0, 16).ToArray();
            byte[] replyCmbTypeNo = BitConverter.GetBytes((ushort)(cmbTypeNo + 10000));

            int jsonDataLength = BitConverter.ToInt32(new ArraySegment<byte>(revBytes, 4, 4).Reverse().ToArray(), 0);

            string jsonData = revBytes.Length != 16 + jsonDataLength ? "" : Encoding.ASCII.GetString(revBytes, 16, jsonDataLength);
            returnHead[8] = replyCmbTypeNo[1];
            returnHead[9] = replyCmbTypeNo[0];

            return new ProtocolData()
            {
                cmdType = cmbTypeNo,
                jsonDataLength = jsonDataLength,
                jsonData = jsonData
            };
        }

        public override async void Server_OnClientMessageRev(object sender, SocketStates _SocketStates)
        {
            try
            {
                byte[] revBytes = _SocketStates.revDataBytes;
                if (revBytes.Length == 0)
                    return;

                ProtocolData protocolData = ProtocolDataParse(revBytes);

                if (protocolData.jsonData != "" && protocolData.jsonData != null)
                    logger.TraceLog($"{protocolData.cmdType}:{protocolData.jsonData}");

                byte[] cmdTypeNoBytes = new byte[2] { revBytes[9], revBytes[8] };
                int cmbTypeNo = BitConverter.ToInt16(cmdTypeNoBytes, 0);

                byte[] oriByt = revBytes.ToArray();
                byte[] returnHead = new ArraySegment<byte>(oriByt, 0, 16).ToArray();
                byte[] replyCmbTypeNo = BitConverter.GetBytes((ushort)(cmbTypeNo + 10000));


                returnHead[8] = replyCmbTypeNo[1];
                returnHead[9] = replyCmbTypeNo[0];

                ResModelBase replyData = await DetermineReplyData(protocolData);

                string json = JsonConvert.SerializeObject(replyData);// System.Text.Json.JsonSerializer.Serialize(replyData);
                int dataLen = json.Length;

                byte[] dataLenBytes = BitConverter.GetBytes(dataLen);
                returnHead[4] = dataLenBytes[3];
                returnHead[5] = dataLenBytes[2];
                returnHead[6] = dataLenBytes[1];
                returnHead[7] = dataLenBytes[0];
                List<byte> replyList = new List<byte>();

                replyList.AddRange(returnHead);
                replyList.AddRange(Encoding.ASCII.GetBytes(json));

                _SocketStates.socket.Send(replyList.ToArray());
            }
            catch (Exception ex)
            {
                logger.ErrorLog(ex);
            }
        }

        virtual protected async Task<ResModelBase> DetermineReplyData(ProtocolData data)
        {
            ResModelBase replyData = null;
            int cmbTypeNo = data.cmdType;
            if (cmbTypeNo == 1000)
                replyData = new robotStatusInfoRes_11000()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    ssid = "AGV",
                    version = "",
                    MAC = "34008AC2ABE4",
                    current_ip = server.hostIP,
                    current_map = "20220817144922441_amb_02",
                    ap_addr = "207693555B32",
                    gyro_version = "f103-1.4.5",
                    rssi = 97,
                    vehicle_id = "AMB-01"


                };
            if (cmbTypeNo == 1002)
                replyData = new robotStatusRunRes_11002()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = ""
                };
            if (cmbTypeNo == 1004)
                replyData = new robotStatusLocRes_11004()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    x = currentPosition[0],
                    y = currentPosition[1],
                    angle = 135
                };
            if (cmbTypeNo == 1005)
                replyData = new robotStatusSpeedRes_11005()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };
            if (cmbTypeNo == 1007)
                replyData = new robotStatusBatteryRes_11007()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    battery_level = DateTime.Now.Second / 100.0
                };

            if (cmbTypeNo == 1110)
            {
                replyData = new robotStatusTaskStatusPackageRes_11110()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    task_status_list = taskStatus.Select(t => new robotStatusTaskStatusPackageRes_11110.TaskStatus { status = t.Value, task_id = t.Key }).ToArray()
                };
            }
            if (cmbTypeNo == 1021)
                replyData = new robotStatusRelocRes_11021()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    reloc_status = (int)relocState
                };
            if (cmbTypeNo == 1020)
            {
                replyData = new robotStatusTaskRes_11020()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    task_type = NavigateState.task_type,
                    task_status = NavigateState.task_status,
                    target_id = NavigateState.target_id,
                    target_point = NavigateState.target_point,
                };

                if (!(replyData as robotStatusTaskRes_11020).isNavigating)
                {
                    MapStatus.Remove(MapStatus.Keys.Last());
                }
            }
            if (cmbTypeNo == 1050)
            {
                replyData = new robotStatusAlarmRes_11050()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    fatals = AgvcAlarm.fatals.Values.Select(fa => fa.ToDictionaryFormat()).ToArray(),
                    errors = AgvcAlarm.errors.Values.Select(fa => fa.ToDictionaryFormat()).ToArray(),
                    warnings = AgvcAlarm.warnings.Values.Select(fa => fa.ToDictionaryFormat()).ToArray(),
                    notices = AgvcAlarm.notices.Values.Select(fa => fa.ToDictionaryFormat()).ToArray()
                };
            }
            if (cmbTypeNo == 1300)
            {
                replyData = new robotStatusMapRes_11300()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    current_map = "20220817144922441_amb_02",
                    current_map_md5 = "1bdc6aef317a8e637a169ec797ba6ae7",
                    maps = new string[] { "20220817144922441_amb_02", "default" }
                };
            }

            if (cmbTypeNo == 1301)
            {
                ROSEmu.ROSCoreAPI ROSService = new ROSEmu.ROSCoreAPI();
                Dictionary<string, ROSEmu.ROSCoreAPI.Point> Stations = ROSService.GetStations().Result;

                var _latStations = Stations.Select(st => new robotStatusStationRes_11301.Station()
                {
                    desc = st.Key,
                    x = st.Value.x,
                    y = st.Value.y,
                    type = "LocationMark",
                    id = st.Key,
                    r = 0
                }).ToArray();

                replyData = new robotStatusStationRes_11301()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    stations = _latStations
                };
            }
            return replyData;
        }

        public Alarm AgvcAlarm { get; set; } = new Alarm();


        public void AddNewAlarm(robotStatusAlarmRes_11050.ALARM_TYPE alarm_type, string code)
        {
            Dictionary<string, AlarmODT> aimObject = new Dictionary<string, AlarmODT>();
            switch (alarm_type)
            {
                case robotStatusAlarmRes_11050.ALARM_TYPE.Fatal:
                    aimObject = AgvcAlarm.fatals;
                    break;
                case robotStatusAlarmRes_11050.ALARM_TYPE.Error:
                    aimObject = AgvcAlarm.errors;
                    break;
                case robotStatusAlarmRes_11050.ALARM_TYPE.Warning:
                    aimObject = AgvcAlarm.warnings;
                    break;
                case robotStatusAlarmRes_11050.ALARM_TYPE.Notice:
                    aimObject = AgvcAlarm.notices;
                    break;
                default:
                    break;
            }

            if (aimObject.TryGetValue(code, out AlarmODT alarm))
            {
                alarm.Times += 1;
                alarm.Timestamp = int.Parse(Math.Round((DateTime.Now - new DateTime(1970, 1, 1, 08, 0, 0)).TotalSeconds) + "");
            }
            else
            {
                aimObject.Add(code, new AlarmODT(code, $"{alarm_type}-{code}")
                {
                    Times = 1,
                    Timestamp = int.Parse(Math.Round((DateTime.Now - new DateTime(1970, 1, 1, 08, 0, 0)).TotalSeconds) + "")
                });
            }
        }

        public class Alarm
        {
            public Dictionary<string, AlarmODT> fatals { get; set; } = new Dictionary<string, AlarmODT>()
            {
                {"50000", new AlarmODT("5000","Fatal-50000") }
            };
            public Dictionary<string, AlarmODT> errors { get; set; } = new Dictionary<string, AlarmODT>()
            {

                {"52201", new AlarmODT("52201","Error-52201") },
                {"52118", new AlarmODT("52118","Error-52118") }
            };
            public Dictionary<string, AlarmODT> warnings { get; set; } = new Dictionary<string, AlarmODT>()
            {
                {"54003", new AlarmODT("54003","Error-54003") },
            };
            public Dictionary<string, AlarmODT> notices { get; set; } = new Dictionary<string, AlarmODT>()
            {

                {"56000", new AlarmODT("56000","Error-56000") },
            };

            internal void Clear()
            {
                foreach (var item in new Dictionary<string, AlarmODT>[] { fatals, errors, warnings, notices })
                {
                    foreach (var alarm in item.Values)
                    {
                        alarm.Times = 0;
                        alarm.Timestamp = 0;
                    }
                }
            }
        }


        public class AlarmODT2 : AlarmODT
        {
            public DateTime OccurTime { get; set; }
            public int Times { get; set; }
        }

        public class AlarmODT
        {
            public string Code { get; set; }
            public string Desc { get; set; }
            public int Timestamp { get; set; }
            public int Times { get; set; }

            public AlarmODT() { }

            public AlarmODT(string code, string desc)
            {
                Code = code;
                Desc = desc;
            }

            public Dictionary<string, object> ToDictionaryFormat()
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                return new Dictionary<string, object>()
                {
                    {Code, Timestamp},
                    {"desc", Desc},
                    {"times", Times},
                };
            }

        }


    }
}
