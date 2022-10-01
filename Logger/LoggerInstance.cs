using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Logger
{
    public class LoggerInstance : ILogger
    {
        public static event EventHandler<LogItem> Onlogging;

        protected string className;

        internal LoggerInstance(Type T)
        {
            className = T.Name;
        }

        internal LoggerInstance()
        {

        }

        public void ErrorLog(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriteLine("Error", ex.Message + $":{message}");
        }

        public void ErrorLog(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriteLine("Error", ex.Message);
        }

        public void FatalLog(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriteLine("Fatal", ex.StackTrace + $":{message}");
        }

        public void FatalLog(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriteLine("Fatal", ex.Message);
        }

        virtual public void InfoLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleWriteLine("Info", message);
        }

        public void TraceLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleWriteLine("Trace", message);
        }

        public void WarnLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            ConsoleWriteLine("Warn", message);
        }

        virtual protected void ConsoleWriteLine(string classify, object message)
        {
            var logItem = new LogItem(DateTime.Now, className, classify, message.ToString());
            Console.WriteLine("{0} | {1} | {2} |: {3}", logItem.Time, logItem.ClassName, logItem.Classify, logItem.Message);
            Onlogging?.Invoke(this, logItem);
        }




        public class LogItem
        {
            public LogItem(DateTime time, string message)
            {
                Time = time;
                Message = message;
            }
            public LogItem(DateTime time, string className, string classify, string message)
            {
                Time = time;
                ClassName = className;
                Classify = classify;
                Message = message;
            }

            public DateTime Time { get; }
            public string ClassName { get; }
            public string Classify { get; }
            public string Message { get; }
        }
    }
}
