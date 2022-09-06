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

        public void ErrorLog(Exception ex, string message)
        {
            throw new NotImplementedException();
        }

        public void ErrorLog(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void FatalLog(Exception ex, string message)
        {
            throw new NotImplementedException();
        }

        public void FatalLog(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void InfoLog(string message)
        {
            throw new NotImplementedException();
        }

        public void TraceLog(string message)
        {
            throw new NotImplementedException();
        }

        public void WarnLog(string message)
        {
            throw new NotImplementedException();
        }
    }
}
