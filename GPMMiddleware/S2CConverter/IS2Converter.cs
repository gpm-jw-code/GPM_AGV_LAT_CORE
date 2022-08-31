using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.S2CConverter
{
    /// <summary>
    /// 派車系統資料轉譯成AGVC資料
    /// </summary>
    internal interface IS2Converter
    {
        void TaskDownloadConvert(object taskObject);
    }
}
