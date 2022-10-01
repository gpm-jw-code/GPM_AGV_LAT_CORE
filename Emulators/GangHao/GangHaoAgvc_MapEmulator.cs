using GangHaoAGV.Models;
using GangHaoAGV.Models.MapModels.Responses;
using GPM_AGV_LAT_CORE.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators.GangHao
{
    public class GangHaoAgvc_MapEmulator : GangHaoAgvc_StateEmulator
    {
        ROSEmu.ROSCoreAPI rosAPI = null;

        public GangHaoAgvc_StateEmulator stateEmu { get; private set; }
        public GangHaoAgvc_MapEmulator(string ip, int port, GangHaoAgvc_StateEmulator stateEmu) : base(ip, port)
        {
            this.stateEmu = stateEmu;
            logger = new LoggerInstance(typeof(GangHaoAgvc_ControlEmulator));
            rosAPI = new ROSEmu.ROSCoreAPI();
        }
        private string[] PathReturn = new string[] { };
        protected override async Task<ResModelBase> DetermineReplyData(ProtocolData data)
        {
            ResModelBase replyData = null;
            int cmbTypeNo = data.cmdType;
            if (cmbTypeNo == 3051)
            {
                stateEmu.AddNewMapReqStates(data.jsonData);
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.jsonData);
                ROSEmu.ROSCoreAPI.WebApiResponse response = await rosAPI.NavigationToStationID(dict);

                replyData = new robotMapTaskGoTargetRes_13051()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = response.code,
                    err_msg = response.message,
                };
            }

            if (cmbTypeNo == 3053) //取得目標站點的路徑
            {
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.jsonData);
                string targetID = dict["id"];
                var PathReturn = await rosAPI.GetTargetPath(targetID);

                replyData = new robotMapTargetPathRes_13053()
                {

                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    path = PathReturn
                };
            }

            if (cmbTypeNo == 3066)
            {
                //stateEmu.AddNewMapReqStates(protocolData.jsonData);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>[]>>(data.jsonData);
                var station1 = dict["move_task_list"][0]["source_id"];
                List<string> stationIDList = new List<string>() { station1 };
                foreach (var item in dict["move_task_list"])
                {
                    stationIDList.Add(item["id"]);
                }

                await rosAPI.NavigationUseTargetList(stationIDList);
                replyData = new robotMapTaskGoTargetListRes_13066()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };
            }
            logger.TraceLog($"|MAP|ACK:{replyData.GetNO()}:{JsonConvert.SerializeObject(replyData)}");
            return replyData;
        }

    }
}
