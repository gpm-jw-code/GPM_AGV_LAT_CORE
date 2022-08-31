using GPM_AGV_LAT_CORE.AGVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.C2SConverter
{
    /// <summary>
    /// 車載數據轉平台所需
    /// </summary>
    internal interface IC2SConverter
    {
        /// <summary>
        /// AGVC 狀態數據轉成AGVS定義的數據
        /// </summary>
        /// <param name="agvc"></param>
        /// <returns></returns>
        object StateConvert(IAGVC agvc);
    }
}
