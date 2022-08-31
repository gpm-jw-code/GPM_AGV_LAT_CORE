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

        public HandshakeResponseDataHelper(string remoteASCIIRev)
        {
            Jsonstr = remoteASCIIRev.Replace("*CR", "");
            remoteObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(Jsonstr);
        }

        public Dictionary<string, object> Create0102MessageData(int RemoteMode)
        {
            _remoteObj["Header"] = new Dictionary<string, object>()
            {
                {"0102",new Dictionary<string, object>()
                {
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
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
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
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
                    {"Time stamp",DateTime.Now.ToString("yyyyMMdd HH:mm:ss") },
                    {"Return Code",ReturnCode }
                } }
            };
            return _remoteObj;
        }
    }
}
