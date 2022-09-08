using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Logger
{
    internal class LoggerInstance : ILogger
    {
        private string className;

        internal LoggerInstance(Type T)
        {
            className = T.Name;

        }

        public void ErrorLog(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriteLine("Error", ex.StackTrace + $":{message}");
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

        public void InfoLog(string message)
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

        private void ConsoleWriteLine(string classify, object message)
        {
            Console.WriteLine("{0} | {1} | {2} |: {3}", DateTime.Now.ToString(), className, classify, message);
        }
    }
}
