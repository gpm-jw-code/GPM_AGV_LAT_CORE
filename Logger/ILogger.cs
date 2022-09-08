using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Logger
{
    public interface ILogger
    {
        void TraceLog(string message);
        void InfoLog(string message);
        void WarnLog(string message);
        void ErrorLog(string message, Exception ex);
        void ErrorLog(Exception ex);
        void FatalLog(string message, Exception ex);
        void FatalLog(Exception ex);
    }
}
