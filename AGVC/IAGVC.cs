using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
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
        /// 接受的派車任務清單
        /// </summary>
        List<clsHostOrder> orderList_LAT { get; set; }

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
        /// 與AGV車連線
        /// </summary>
        /// <returns>是否連接成功</returns>
        bool ConnectToAGV();

        /// <summary>
        /// 同步車子狀態
        /// </summary>
        void SyncState();
        /// <summary>
        /// 同步訂單列表
        /// </summary>
        void SyncOrdersState();
        /// <summary>
        /// 同步訂單執行狀況
        /// </summary>
        void SyncSyncOrderExecuteState();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TryExecuteOrder(clsHostOrder order, out string message);

        /// <summary>
        /// 車輛狀態或訂單狀態變化
        /// </summary>
        event EventHandler StateOnChanged;

    }
}
