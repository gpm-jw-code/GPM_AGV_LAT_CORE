using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Parameters
{
    /// <summary>
    /// 派車平台參數
    /// </summary>
    public class AGVSParameters : IParameter
    {
        public TCPParameters tcpParams { get; set; } = new TCPParameters();
    }

}
