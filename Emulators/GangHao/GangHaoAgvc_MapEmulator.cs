using GangHaoAGV.Models;
using GangHaoAGV.Models.ControlModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators.GangHao
{
    public class GangHaoAgvc_MapEmulator : GangHaoAgvc_StateEmulator
    {
        public GangHaoAgvc_MapEmulator(string ip, int port) : base(ip, port)
        {
        }

        protected override ResModelBase DetermineReplyData(int cmbTypeNo)
        {
            ResModelBase replyData = null;

            //if (cmbTypeNo == 2002)
            //    replyData = new robotControlRelocRes_12002()
            //    {
            //        create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
            //        ret_code = 0,
            //        err_msg = "",
            //    };
            //if (cmbTypeNo == 2003)
            //    replyData = new robotControlConfirmlocRes_12003()
            //    {
            //        create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
            //        ret_code = 0,
            //        err_msg = "",
            //    };
            return replyData;
        }
    }
}
