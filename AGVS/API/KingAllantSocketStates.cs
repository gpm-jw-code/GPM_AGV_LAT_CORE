using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;

namespace GPM_AGV_LAT_CORE.AGVS.API
{
    public class KingAllantSocketStates : SocketStates
    {
        public KingAllantSocketStates() : base()
        {

        }

        public string JSONCmd => ASCIIRev.Replace("*CR", "");
    }
}
