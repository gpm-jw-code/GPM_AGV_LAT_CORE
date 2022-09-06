using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public class clsOrderAGVCinfo
    {
        public AGVC_TYPES agvcType { get; private set; }
        public string agvcID { get; private set; }

        public IAgvcInfoToAgvs agvcInfoForagvs { get; private set; }

        public clsOrderAGVCinfo(AGVC_TYPES agvcType, string agvcID, IAgvcInfoToAgvs agvcInfoForagvs)
        {
            this.agvcType = agvcType;
            this.agvcID = agvcID;
            this.agvcInfoForagvs = agvcInfoForagvs;
        }
    }
}
