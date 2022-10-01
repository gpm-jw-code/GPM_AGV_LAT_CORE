using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVC.AGVCInfo;
using GPM_AGV_LAT_CORE.AGVS.API;
using GPM_AGV_LAT_CORE.GPMMiddleware;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using GPM_AGV_LAT_CORE.LATSystem;
using GPM_AGV_LAT_CORE.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVS
{
    internal class GpmAGVS : IAGVS
    {
        public AGVS_TYPES agvsType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string VenderName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AGVSParameters agvsParameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IAGVC> RegistedAgvcList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IAgvsHandShakeable agvsApi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IAgvcInfoToAgvs> BindingAGVCInfoList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<clsHostExecuting> ExecuteTaskList { get; set; } = new List<clsHostExecuting>();

        public event EventHandler<object> OnHostMessageReceived;
        public event EventHandler<IAGVSExecutingState> OnTaskDownloadRecieved;//<


        public bool ConnectToHost(out string err_msg)
        {
            throw new NotImplementedException();

        }

        public Task<bool> ReportAGVCState(IAGVC agvc, AGVCStateStore agvcState)
        {
            throw new NotImplementedException();
        }

        public class GPMExcutingState : IAGVSExecutingState
        {
            public dynamic executingObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ORDER_STATE state { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
