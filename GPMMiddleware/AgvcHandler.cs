using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.GPMMiddleware.C2SConverter;
using GPM_AGV_LAT_CORE.LATSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware
{

    /// <summary>
    /// 處理AGVC事件
    /// </summary>
    public static class AgvcHandler
    {
        /// <summary>
        /// Handle GangHao AGVC 狀態改變
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void GangHaoAgvcStateChangedHandle(object sender, EventArgs e)
        {
            IC2SConverter convert = ConverterSelect((IAGVC)sender);
            AGVSManager.CurrentAGVS.ReportAGVCState(convert.StateConvert((IAGVC)sender));
        }

        /// <summary>
        /// Handle GPM AGVC 狀態改變
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void GPMAgvcStateChangedHandle(object sender, EventArgs e)
        {
            IC2SConverter convert = ConverterSelect((IAGVC)sender);
            AGVSManager.CurrentAGVS.ReportAGVCState(convert.StateConvert((IAGVC)sender));
        }


        /// <summary>
        /// 由AGVC廠牌與AGVS廠牌決定要用哪一個轉換器
        /// </summary>
        /// <param name="agvc"></param>
        /// <returns></returns>
        private static IC2SConverter ConverterSelect(IAGVC agvc)
        {
            IC2SConverter converter = null;
            var agvcType = agvc.agvcType;
            if (SystemParams.AgvsTypeToUse == AGVS_TYPES.KINGGALLENT)
            {
                switch (agvcType)
                {
                    case AGVC_TYPES.GPM:
                        converter = new GangHaoAgvc2GPMAgvsConverter();
                        break;
                    case AGVC_TYPES.GangHau:
                        converter = new GangHaoAgvc2KingGallentAgvsConverter();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (agvcType)
                {
                    case AGVC_TYPES.GPM:
                        converter = new GPMAgvc2GPMAgvsConverter();
                        break;
                    case AGVC_TYPES.GangHau:
                        converter = new GPMAgvc2KingGallentAgvsConverter();
                        break;
                    default:
                        break;
                }
            }

            return converter;
        }

    }
}
