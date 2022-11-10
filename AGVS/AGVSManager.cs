using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    /// <summary>
    /// AGVS管理、中繼者
    /// </summary>
    public static class AGVSManager
    {

        /// <summary>
        /// 目前使用的派車平台
        /// </summary>
        public static IAGVS CurrentAGVS;




        /// <summary>
        /// 連接到系統參數所指定的派車平台
        /// </summary>
        public static async Task ConnectToHosts()
        {

            if (SystemParams.AgvsTypeToUse == AGVS_TYPES.KINGGALLENT)
            {
                CurrentAGVS = new KingGallentAGVS()
                {
                    BindingAGVCInfoList = new List<IAgvcInfoToAgvs>()
                    {
                         new AgvcInfoForKingAllant(){  AGVID="001", StationID="001", EQID="001", EQName="AGV_001"},
                         new AgvcInfoForKingAllant(){  AGVID="002", StationID="001", EQID="002", EQName="AGV_002"},
                         new AgvcInfoForKingAllant(){  AGVID="003", StationID="001", EQID="003", EQName="AGV_003"},
                    },
                    agvsParameters = new Parameters.AGVSParameters
                    {
                        tcpParams = new Parameters.TCPParameters
                        {
                            HostIP = "0.tcp.jp.ngrok.io", //127.0.0.1
                            HostPort = 14776,
                            LocalIP = "172.20.10.8",
                            LocalPort = 13001
                        }
                    }
                };
                CurrentAGVS.OnTaskDownloadRecieved += GPMMiddleware.AgvsHandler.KingGellantAGVSTaskDownloadHandle;///註冊派車任務下載事件
            }
            else if (SystemParams.AgvsTypeToUse == AGVS_TYPES.GPM)
            {
                //...
                CurrentAGVS.OnTaskDownloadRecieved += GPMMiddleware.AgvsHandler.GPMAGVSTaskDownloadHandle;///註冊派車任務下載事件
            }

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
