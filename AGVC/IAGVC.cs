using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Parameters;

namespace GPM_AGV_LAT_CORE.AGVC
{
    /// <summary>
    /// 這是一部車AGV
    /// </summary>
    public interface IAGVC
    {

        string ID { get; set; }
        int Index { get; set; }
        string EQName { get; }
        /// <summary>
        /// 車子廠商
        /// </summary>
        AGVC_TYPES agvcType { get; set; }

        /// <summary>
        /// 與AGV車連線
        /// </summary>
        /// <returns>是否連接成功</returns>
        bool ConnectToAGV();
        /// <summary>
        /// AGVC 參數
        /// </summary>
        AGVCParameters agvcParameters { get; set; }

        /// <summary>
        /// AGVC狀態
        /// </summary>
        AGVCStateStore agvcStates { get; set; }

        IAgvcInfoToAgvs agvcInfos { get; set; }

        /// <summary>
        /// 從車子Download抓取目前的狀態
        /// </summary>
        void FetchCurrentState();

        void Emulation();

        /// <summary>
        /// 車輛狀態或訂單狀態變化
        /// </summary>
        event EventHandler StateOnChanged;

    }
}
