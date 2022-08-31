using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    /// <summary>
    /// 這是一套派車系統
    /// </summary>
    public interface IAGVS
    {
        #region 屬性欄位
        /// <summary>
        /// 平台廠商
        /// </summary>
        AGVS_TYPES agvsType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string VenderName { get; set; }

        /// <summary>
        /// 參數
        /// </summary>
        AGVSParameters agvsParameters { get; set; }



        /// <summary>
        /// 是否已經跟平台連線
        /// </summary>
        bool connected { get; set; }

        /// <summary>
        /// 註冊的車子
        /// </summary>
        List<IAGVC> RegistedAgvcList { get; set; }

        /// <summary>
        /// 跟派車系統交握的API介面
        /// </summary>
        IAgvsApi agvsApi { get; set; }

        /// <summary>
        /// 需要上系統的AGV車輛參數組
        /// </summary>
        List<IAgvcInfoToAgvs> BindingAGVCInfoList { get; set; }

        #endregion

        #region 方法
        /// <summary>
        /// 與派車系統連線
        /// </summary>
        /// <returns></returns>
        bool ConnectToHost(out string err_msg);


        /// <summary>
        /// 上報AGV車狀態
        /// </summary>
        /// <param name="agvcState"></param>
        void ReportAGVCState(object agvcState);

        #endregion

        #region 事件

        /// <summary>
        /// 收到派車系統指令/數據觸發的事件
        /// </summary>
        event EventHandler<object> OnHostMessageReceived;

        /// <summary> 
        /// 收到派車系統 Task Execute事件
        /// </summary>
        event EventHandler<object> OnTaskDownloadRecieved;

        #endregion

    }
}
