using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    public static class AGVSManager
    {

        public static IAGVS CurrentAGVS;

        /// <summary>
        /// 連接到系統參數所指定的派車平台
        /// </summary>
        public static async Task ConnectToHosts()
        {

            if (SystemParams.AgvsTypeToUse == AGVS_TYPES.KINGGALLENT)
                CurrentAGVS = new KingGallentAGVS()
                {
                    BindingAGVCInfoList = new List<IAgvcInfoToAgvs>()
                    {
                         new AgvcInfoForKingAllant(){  AGVID="001", StationID="001", EQID="001", EQName="AGV_001"},
                         new AgvcInfoForKingAllant(){  AGVID="002", StationID="001", EQID="001", EQName="AGV_002"},
                    },
                    agvsParameters = new Parameters.AGVSParameters
                    {
                        tcpParams = new Parameters.TCPParameters
                        {
                            HostIP = "127.0.0.1",
                            HostPort = 5500
                        }
                    }
                };


            CurrentAGVS.OnTaskDownloadRecieved += GPMMiddleware.AgvsHandler.TaskDownloadHandle;
            await TryConnectToHost();
        }


        private static async Task TryConnectToHost()
        {
            int connectTryNum = 1;
            while (!CurrentAGVS.ConnectToHost(out string errMsg))
            {
                Console.WriteLine("嘗試與派車系統({0})連線 Fail({1})...({2})", CurrentAGVS.agvsType, errMsg, connectTryNum);
                await Task.Delay(TimeSpan.FromSeconds(1));
                connectTryNum++;
            }
            Console.WriteLine("與派車系統({0})連線成功!", CurrentAGVS.agvsType);
        }

    }
}
