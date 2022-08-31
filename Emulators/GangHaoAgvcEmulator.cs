using GangHaoAGV.Models.StateModels.Responses;
using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.Protocols.Tcp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Emulators
{
    public class GangHaoAgvcEmulator : KingGallentAgvsEmulator
    {

        private int LocationXInitial = 0;
        private int LocationYInitial = 0;

        public GangHaoAgvcEmulator(string ip, int port) : base(ip, port)
        {
            LocationXInitial = new Random().Next(200, 300);
            LocationYInitial = new Random().Next(100, 700);
        }



        public override void Server_OnClientMessageRev(object sender, SocketStates _SocketStates)
        {
            //
            byte[] revBytes = _SocketStates.revDataBytes;
            byte[] cmdTypeNoBytes = new byte[2] { revBytes[9], revBytes[8] };
            int cmbTypeNo = BitConverter.ToInt16(cmdTypeNoBytes, 0);

            byte[] returnHead = revBytes.ToArray();
            byte[] replyCmbTypeNo = BitConverter.GetBytes((ushort)(cmbTypeNo + 10000));
            returnHead[8] = replyCmbTypeNo[1];
            returnHead[9] = replyCmbTypeNo[0];
            ResModelBase replyData = DetermineReplyData(cmbTypeNo);
            string json = JsonConvert.SerializeObject(replyData);
            int dataLen = json.Length;

            byte[] dataLenBytes = BitConverter.GetBytes(dataLen);
            returnHead[4] = dataLenBytes[3];
            returnHead[5] = dataLenBytes[2];
            returnHead[6] = dataLenBytes[1];
            returnHead[7] = dataLenBytes[0];
            List<byte> replyList = new List<byte>();
            replyList.AddRange(returnHead);
            replyList.AddRange(Encoding.ASCII.GetBytes(json));

            _SocketStates.socket.Send(replyList.ToArray());

        }

        private ResModelBase DetermineReplyData(int cmbTypeNo)
        {
            ResModelBase replyData = null;
            if (cmbTypeNo == 1000)
                replyData = new robotStatusInfoRes_11000()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = ""
                };
            if (cmbTypeNo == 1002)
                replyData = new robotStatusRunRes_11002()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = ""
                };
            if (cmbTypeNo == 1004)
                replyData = new robotStatusLocRes_11004()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    x = LocationXInitial + DateTime.Now.Second,
                    y = LocationYInitial + DateTime.Now.Second,
                    r = 135
                };
            if (cmbTypeNo == 1005)
                replyData = new robotStatusSpeedRes_11005()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };
            if (cmbTypeNo == 1007)
                replyData = new robotStatusBatteryRes_11007()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                    remainPercent = DateTime.Now.Second
                };

            if (cmbTypeNo == 1110)
                replyData = new robotStatusTaskStatusPackageRes_11110()
                {
                    create_on = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    ret_code = 0,
                    err_msg = "",
                };

            return replyData;
        }


    }
}
