using System;
using System.Collections.Generic;

namespace GPM_AGV_LAT_CORE.Emulators.KingGallentAGVS
{

    public partial class KingGallentAgvsEmulator
    {

        public class OrderResult
        {
            public enum RUN_STATE
            {
                EXECUTING,
                WAITING,
                CANCELED,
                FAIL
            }
            public OrderResult(bool Success, RUN_STATE State)
            {
                this.Success = Success;
                this.State = State;
            }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public RUN_STATE State { get; set; }
        }

    }
}
