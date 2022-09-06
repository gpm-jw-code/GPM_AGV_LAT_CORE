using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangHaoAGV.Models.Order;

namespace GPM_AGV_LAT_CORE.AGVC.Order
{
    public class GangHaoAGVCOrder : SetOrder, IAGVCOrder 
    {
        public string comment { get; set; }
    }
}
