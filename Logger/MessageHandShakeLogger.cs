using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPM_AGV_LAT_CORE.Logger.MessageHandShakeLogger.HandshakeLogItem;

namespace GPM_AGV_LAT_CORE.Logger
{
    public class MessageHandShakeLogger : LoggerInstance
    {
        public static event EventHandler<HandshakeLogItem> OnHandShakelogging;

        internal MessageHandShakeLogger(Type T) : base(T)
        {
        }

        internal MessageHandShakeLogger()
        {
            className = "MessageHandShakeLogger";
        }

        internal void AGVSToLAT(string message)
        {
            logimp(DIRECTION.AGVS_TO_LAT, message);
        }
        internal void LATToAGVS(string message)
        {
            logimp(DIRECTION.LAT_TO_AGVS, message);
        }
        internal void AGVCToLAT(string message)
        {
            logimp(DIRECTION.AGVC_TO_LAT, message);
        }
        internal void LATToAGVC(string message)
        {
            logimp(DIRECTION.LAT_TO_AGVC, message);
        }

        void logimp(DIRECTION direction, string message)
        {
            HandshakeLogItem logItem = new HandshakeLogItem(DateTime.Now, message, direction);
            OnHandShakelogging?.Invoke(this, logItem);

            //Console.ForegroundColor = ConsoleColor.DarkYellow;
            //Console.WriteLine("{0}|MessageHandShake|{1}|{2}", logItem.Time, logItem.direction, logItem.Message);
        }
        public class HandshakeLogItem : LogItem
        {
            internal DIRECTION direction;
            public string Direction => direction.ToString();
            public enum DIRECTION
            {
                LAT_TO_AGVS,
                AGVS_TO_LAT,
                LAT_TO_AGVC,
                AGVC_TO_LAT
            }

            public HandshakeLogItem(DateTime time, string className, string classify, string message) : base(time, className, classify, message)
            {
            }
            public HandshakeLogItem(DateTime time, string message, DIRECTION direction) : base(time, message)
            {
                this.direction = direction;
            }
        }
    }
}
