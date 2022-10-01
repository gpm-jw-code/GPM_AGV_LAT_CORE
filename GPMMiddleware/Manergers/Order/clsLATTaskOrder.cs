using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public class clsLATTaskOrder
    {
        /// <summary>
        /// 任務名稱(global唯一)
        /// </summary>
        public string taskName { get; set; }

        /// <summary>
        /// 被指派的AGV名稱
        /// </summary>
        public string executEqName { get; set; }

        /// <summary>
        /// 任務動作列表
        /// </summary>
        public clsAction action { get; set; } = new clsAction();


        public class clsAction
        {
            public enum ACTION_TYPE
            {
                NAVIGATOR
            }
            /// <summary>
            /// 動作ID(唯一)
            /// </summary>
            public string actionID { get; set; }
            /// <summary>
            /// 目標點位
            /// </summary>
            public string stationID { get; set; }
            /// <summary>
            /// 動作參數
            /// </summary>
            public Dictionary<string, object> operation_args { get; set; } = new Dictionary<string, object>();

            public List<string> paths { get; set; } = new List<string>();

        }
    }
}
