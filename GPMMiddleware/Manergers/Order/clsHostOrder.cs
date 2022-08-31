using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order
{
    /// <summary>
    /// 派工任務訂單
    /// </summary>
    public class clsHostOrder : INotifyPropertyChanged
    {
        private DateTime _CompleteTimeStamp;
        private ORDER_STATE _State = ORDER_STATE.WAIT_EXECUTE;
        /// <summary>
        /// 訂單流水號
        /// </summary>
        public int OrderNo { get; set; }
        /// <summary>
        /// 訂單狀態
        /// </summary>
        public ORDER_STATE State
        {
            get => _State;
            set
            {
                if (value != _State)
                {
                    _State = value;
                    NotifyPropertyChanged();
                    if (_State == ORDER_STATE.COMPLETE)
                    {
                        CompleteTimeStamp = DateTime.Now;
                    }
                }
            }
        }
        /// <summary>
        /// 執行訂單的AGVC
        /// </summary>
        public IAGVC ExecuteingAGVC { get; set; }
        /// <summary>
        /// 發派任務的AGVS
        /// </summary>
        public IAGVS FromAGVS { get; set; }
        /// <summary>
        /// 從遠端接收的時間
        /// </summary>
        public DateTime RecieveTimeStamp { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime CompleteTimeStamp
        {
            get => _CompleteTimeStamp;
            private set
            {
                if (value != _CompleteTimeStamp)
                {
                    _CompleteTimeStamp = value;
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 遠端發送任務物件
        /// </summary>
        public object TaskDownloadData { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName) { });
        }

    }
}
