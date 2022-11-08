using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCStates
{
    public enum OPERATION_STATE
    {
        AUTO, MANUAL
    }

    public enum RUNNING_STATE
    {
        IDLE, RUNNING
    }

    public enum ONLINE_STATE
    {
        ONLINE, OFFLINE, Unknown,Downloading
    }

    public enum CONNECTION_STATE
    {
        CONNECTED, CONNECTING, DISCONNECT
    }


    /// <summary>
    /// 
    /// </summary>
    public class AGVCRUNStates
    {
        public CONNECTION_STATE EConnectionState { get; set; } = CONNECTION_STATE.DISCONNECT;
        public OPERATION_STATE EOperationState { get; set; } = OPERATION_STATE.MANUAL;
        public RUNNING_STATE ERunningState { get; set; } = RUNNING_STATE.IDLE;
        public ONLINE_STATE EOnlineState { get; set; } = ONLINE_STATE.Unknown;

        public string ConnectionState => EConnectionState.ToString();
        public string OperationState => EOperationState.ToString();
        public string RunningState => ERunningState.ToString();
        public string OnlineState => EOnlineState.ToString();

    }

}
