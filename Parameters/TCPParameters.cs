using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Parameters
{
    /// <summary>
    /// TCP 參數組
    /// </summary>
    public class TCPParameters
    {
        public string HostIP { get; set; } = "192.168.0.107";
        public int HostPort { get; set; } = 5010;

        public string LocalIP { get; set; } = "127.0.0.1";
        public int LocalPort { get; set; } = 3001;

        public string Host => $"{HostIP}:{HostPort}";
    }
}
