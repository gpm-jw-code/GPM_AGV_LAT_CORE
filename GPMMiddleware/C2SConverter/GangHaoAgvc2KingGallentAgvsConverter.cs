
using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.Models.KingAllant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.C2SConverter
{
    internal class GangHaoAgvc2KingGallentAgvsConverter : IC2SConverter
    {
        public object StateConvert(IAGVC agvc)
        {
            AGVC.GangHaoAGV _agvc = (AGVC.GangHaoAGV)agvc;
            AgvcInfoForKingAllant info = (AgvcInfoForKingAllant)_agvc.agvcInfos;
            HandshakeRunningStatusReportHelper helper = new HandshakeRunningStatusReportHelper(info.SID, info.EQName);
            var stateReportData = helper.CreateStateReportDataModel(new RunningStateReportModel());
            return stateReportData;
        }
    }
}
