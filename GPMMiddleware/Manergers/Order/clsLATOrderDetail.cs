using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    public class clsLATOrderDetail
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
        public List<clsAction> actions { get; set; } = new List<clsAction>();
        /// <summary>
        /// 任務是否封口 (封口:不可再添加新的動作 ; 不封口:可再添加新的動作)
        /// </summary>
        public bool complete { get; set; } = false;


        public class clsAction
        {
            /// <summary>
            /// 動作ID(唯一)
            /// </summary>
            public string actionID { get; set; }
            /// <summary>
            /// 目標點位
            /// </summary>
            public string location { get; set; }
            /// <summary>
            /// 動作參數
            /// </summary>
            public Dictionary<string, object> operation_args { get; set; }=new Dictionary<string, object>();

        }
    }
}
