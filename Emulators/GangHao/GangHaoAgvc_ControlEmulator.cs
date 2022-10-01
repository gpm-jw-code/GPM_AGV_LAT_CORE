using GangHaoAGV.Models;
using GangHaoAGV.Models.ControlModels.Responses;
using GPM_AGV_LAT_CORE.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators.GangHao
{
    public class GangHaoAgvc_ControlEmulator : GangHaoAgvc_StateEmulator
    {
        public GangHaoAgvc_ControlEmulator(string ip, int port) : base(ip, port)
        {
            logger = new LoggerInstance(typeof(GangHaoAgvc_ControlEmulator));
        }

        protected override async Task<ResModelBase> DetermineReplyData(ProtocolData data)
        {
            ResModelBase replyData = null;
            int cmdTypeNo = data.cmdType;
            if (cmdTypeNo == 2002)
                replyData = new robotControlRelocRes_12002()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };
            if (cmdTypeNo == 2003)
                replyData = new robotControlConfirmlocRes_12003()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };
            return replyData;
        }
    }
}
