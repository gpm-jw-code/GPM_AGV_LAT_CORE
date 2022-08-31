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
        internal static void AgvcStateChangedHandle(object sender, EventArgs e)
        {
            IC2SConverter convert = ConverterSelect((IAGVC)sender);
            StateReport(convert.StateConvert((IAGVC)sender));
        }

        private static void StateReport(object v)
        {
            AGVSManager.CurrentAGVS.ReportAGVCState(v);
        }

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
