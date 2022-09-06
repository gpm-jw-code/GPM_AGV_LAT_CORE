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
        void ErrorLog(Exception ex, string message);
        void ErrorLog(Exception ex);
        void FatalLog(Exception ex, string message);
        void FatalLog(Exception ex);
    }
}
