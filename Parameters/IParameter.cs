using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Parameters
{
    public interface IParameter
    {
        /// <summary>
        /// TCP參數(比如KingAllant就要用TCP連線 ; GangHao AGV底層控制用TCP)
        /// </summary>
        TCPParameters tcpParams { get; set; }
    }
}
