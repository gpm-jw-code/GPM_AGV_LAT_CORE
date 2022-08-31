using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Parameters
{
    public class AGVCParameters : IParameter
    {
        public TCPParameters tcpParams { get; set; } = new TCPParameters();
    }
}
