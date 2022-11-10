using GPM_AGV_LAT_CORE.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS.Models.KingAllant
{
    public class HandshakeResponseDataHelper
    {
        public string Jsonstr { get; set; }
        private Dictionary<string, object> _remoteObj;
        private Dictionary<string, object> returnObj;
        ILogger logger = new LoggerInstance(typeof(HandshakeResponseDataHelper));
        public Dictionary<string, object> remoteObj
        {
            get => _remoteObj;
            private set
            {
                _remoteObj = value;
                returnObj = new Dictionary<string, object>() {
                    {"SID",_remoteObj["SID"]},
                    {"EQName",_remoteObj["EQName"]},
                    {"System Bytes",_remoteObj["System Bytes"]},
                    {"Header", new Dictionary<string,object>() },
                };
            }
        }



        public HandshakeResponseDataHelper(Dictionary<string, object> remoteTaskObj)
        {
            remoteObj = remoteTaskObj;
        }

        public HandshakeResponseDataHelper()
        {

        }

        internal bool CreateTemplate(string Jsonstr)
        {
            try
            {
                var remoteObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(Jsonstr);
                _remoteObj = new Dictionary<string, object>() {
                    {"SID",remoteObj["SID"]},
                    {"EQName",remoteObj["EQName"]},
                    {"System Bytes",remoteObj["System Bytes"]},
                    {"Header", new Dictionary<string,object>() },
                };
                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorLog("HandshakeResponseDataHelper Error occur in construtor", ex);
                return false;
            }
        }

        public Dictionary<string, object> Create0102MessageData(int RemoteMode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0102",new Dictionary<string, object>()
                {
                    {"Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    {"Remote Mode",RemoteMode }
                } }
            };
            return _remoteObj;
        }

        /// <summary>
        /// Online/Offline Request Acknowledg
        /// </summary>
        /// <param name="ReturnCode">0:可以上線下線;1:上線失敗</param>
        /// <returns></returns>
        public Dictionary<string, object> Create0104MessageData(int ReturnCode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0104",new Dictionary<string, object>()
                {
                    {"Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    {"Return Code",ReturnCode }
                } }
            };
            return _remoteObj;
        }

        internal Dictionary<string, object> create0106MessageData(int ReturnCode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0106",new Dictionary<string, object>()
                {
                    {"Time Stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    {"Return Code",ReturnCode }
                } }
            };
            return _remoteObj;
        }

        /// <summary>
        /// Task Download Acknowledge(0301的回覆)
        /// </summary>
        /// <param name="ReturnCode"></param>
        /// <returns></returns>
        internal Dictionary<string, object> create0302MessaeData(int ReturnCode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0302",new Dictionary<string, object>()
                {
                    {"Return Code",ReturnCode }
                } }
            };
            return _remoteObj;
        }




        /// <summary>
        /// Task Feedback Acknowledge(0303的回覆)
        /// </summary>
        /// <param name="ReturnCode"> 0:OK ; Others: NG ,Error</param>
        /// <returns></returns>
        internal Dictionary<string, object> create0304MessaeData(int ReturnCode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0304",new Dictionary<string, object>()
                {
                    {"Return Code",ReturnCode }
                } }
            };
            return _remoteObj;
        }
    }
}
