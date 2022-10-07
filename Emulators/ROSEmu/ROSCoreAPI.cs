using GangHaoAGV.Models.StateModels.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators.ROSEmu
{
    public class ROSCoreAPI
    {
        Logger.LoggerInstance logger = new Logger.LoggerInstance(typeof(ROSCoreAPI));
        internal string hostURI { get; } = "http://127.0.0.1:7122/api/TurtleBot3";
        public ROSCoreAPI()
        {
        }

        public async Task NavigationToStationID(string stationID)
        {
            await HttpPost($"/Navigator_Station?stationID={stationID}");
        }
        internal async Task<WebApiResponse> NavigationToStationID(Dictionary<string, string> dict)
        {
            string resJson = await HttpPost("/NavigatorTask", dict);
            var response = JsonConvert.DeserializeObject<WebApiResponse>(resJson);
            return response;
        }
        public async Task NavigationUseTargetList(List<string> stationIDList)
        {
            await HttpPost("/NavigatorUseTargetList", stationIDList);
        }

        internal async Task PauseNavigate()
        {
            await HttpPost("/PauseNavigate");
        }

        internal async Task ResumeNavigate()
        {
            await HttpPost("/ResumeNavigate");
        }

        internal async Task<robotStatusTaskRes_11020> GetNavigatingState()
        {
            var res = await HttpGet($"/NavigatingState");
            if (res == null)
                return new robotStatusTaskRes_11020
                {

                };
            var navigatState = JsonConvert.DeserializeObject<robotStatusTaskRes_11020>(res);
            return navigatState;
        }

        internal async Task CancelNavigating()
        {
            var response = await HttpPost("/CancleNavigatingTask");
            Console.WriteLine(response);
        }

        internal async Task<double[]> GetCurrentPosition()
        {

            var json = await HttpGet("/CurrentPosition");
            return JsonConvert.DeserializeObject<double[]>(json);
        }

        internal async Task<Dictionary<string, int>> GetTaskStatus()
        {
            var json = await HttpGet("/TaskStatus");
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        }
        /// <summary>
        /// 取得目標站點
        /// </summary>
        /// <returns></returns>
        internal async Task<Dictionary<string, Point>> GetStations()
        {
            var res = await HttpGet("/Stations");
            var stations = JsonConvert.DeserializeObject<Dictionary<string, Point>>(res);
            return stations;
        }

        /// <summary>
        /// 取得目標站點行經的點位ID
        /// </summary>
        /// <param name="targetID"></param>
        /// <returns></returns>
        internal async Task<string[]> GetTargetPath(string targetID)
        {
            var res = await HttpGet($"/TargetPath?targetID={targetID}");
            var path = JsonConvert.DeserializeObject<string[]>(res);
            return path;
        }

        #region Private Methods

        private async Task<string> HttpGet(string path)
        {
            //logger.TraceLog($"|HTTP_GET|SEND| {path}");
            try
            {
                string uri = this.hostURI + path;
                using (var client = new HttpClient())
                {
                    var result = await client.GetAsync(uri);
                    var statusCode = result.StatusCode;
                    var jsonresult = result.Content.ReadAsStringAsync().Result;
                    return jsonresult;
                }
            }
            catch (Exception ex)
            {
                logger.TraceLog($"|HTTP_GET|ERROR| {ex.Message}");
                return null;
            }
        }



        private async Task<string> HttpPost(string path, object body = null)
        {
            try
            {
                string uri = this.hostURI + path;
                string bodyJson = body == null ? "" : JsonConvert.SerializeObject(body);
                HttpContent content = new StringContent(bodyJson);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string responseJsonstr = "";
                using (var client = new HttpClient())
                {
                    var result = await client.PostAsync(uri, content);
                    var statusCode = result.StatusCode;
                    responseJsonstr = await result.Content.ReadAsStringAsync();
                }
                return responseJsonstr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        public class NavigatingState
        {
            /// <summary>
            /// 是否進行導航任務
            /// </summary>
            public bool IsNavigating { get; set; } = false;
            public bool Disconnected { get; set; } = false;
        }
        public class Point
        {
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }

        public class WebApiResponse
        {
            public string message { get; set; }
            public int code { get; set; }
        }
    }
}
