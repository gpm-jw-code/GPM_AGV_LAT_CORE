using GPM_AGV_LAT_CORE.AGVC;
using GPM_AGV_LAT_CORE.AGVS;
using GPM_AGV_LAT_CORE.GPMMiddleware.Manergers.Order;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static GPM_AGV_LAT_CORE.GPMMiddleware.AgvsHandler;

namespace GPM_AGV_LAT_CORE.GPMMiddleware.ExcutingPreProcessor
{
    public enum EXECUTE_TYPE
    {
        /// <summary>
        /// 訂單執行
        /// </summary>
        Order,
        /// <summary>
        /// 終止任務
        /// </summary>
        Reset,
        UnKnown
    }

    interface IAgvsExcutingPreProcessor
    {
        IAGVSExecutingState aGVSExecutingState { get; set; }
        dynamic taskDownloadObject { get; set; }
        EXECUTE_TYPE EExecuteType { get; set; }
        IAGVC agvcFound { get; set; }
        IAGVC FindAGV();
        clsLATOrderDetail OrderConvertToLATFormat();
        /// <summary>
        /// 判斷派車指令類型
        /// </summary>
        /// <returns></returns>
        EXECUTE_TYPE JudgeExecutingType();
        clsHostExecuting Run(IAGVS agvs, IAGVSExecutingState OrderObjectFromAgvs);
    }



    internal class AgvsExcutingPreProcessorBase : IAgvsExcutingPreProcessor
    {

        public IAGVSExecutingState aGVSExecutingState { get; set; }

        public IAGVC agvcFound { get; set; }
        public EXECUTE_TYPE EExecuteType { get; set; }
        public dynamic taskDownloadObject { get; set; }

        virtual public clsLATOrderDetail OrderConvertToLATFormat()
        {
            throw new NotImplementedException();
        }

        virtual public IAGVC FindAGV()
        {
            throw new NotImplementedException();
        }

        virtual public EXECUTE_TYPE JudgeExecutingType()
        {
            throw new NotImplementedException();
        }

        virtual public clsHostExecuting Run(IAGVS agvs, IAGVSExecutingState aGVSExecutingState)
        {
            this.aGVSExecutingState = aGVSExecutingState;
            EExecuteType = JudgeExecutingType();
            agvcFound = FindAGV();
            clsHostExecuting newExecuting = new clsHostExecuting(agvs, agvcFound, taskDownloadObject, EExecuteType)
            {
                RecieveTimeStamp = DateTime.Now,
                State = ORDER_STATE.WAIT_EXECUTE,
            };

            if (EExecuteType == EXECUTE_TYPE.Order)
            {
                clsLATOrderDetail latOrder = OrderConvertToLATFormat();
                newExecuting.latOrderDetail = latOrder;
            }

            Console.WriteLine("AGV TaskDownload From {0} to {1} |Task Content: {2}", agvs.agvsType, agvcFound == null ? "No-Car-Match" : agvcFound.GetType().Name, JsonConvert.SerializeObject(aGVSExecutingState.executingObject));
            return newExecuting;
        }
    }

    internal class KingGallentExcutingPreProcessor : AgvsExcutingPreProcessorBase
    {
        Dictionary<string, object> _taskObj;
        Dictionary<string, Dictionary<string, object>> _headerData;
        public override EXECUTE_TYPE JudgeExecutingType()
        {
            if (_headerData.ContainsKey("0301"))
                return EXECUTE_TYPE.Order;
            else if (_headerData.ContainsKey("0305"))
                return EXECUTE_TYPE.Reset;
            else
                return EXECUTE_TYPE.UnKnown;
        }
        public override clsLATOrderDetail OrderConvertToLATFormat()
        {
            this.taskDownloadObject = _taskObj;
            return OrderConverter.AGVSToLAT.KingGallentOrderToLATOrder(_taskObj);
        }

        public override IAGVC FindAGV()
        {
            //解析
            string SID = _taskObj["SID"].ToString();
            IAGVC agvc = AGVCManager.FindAGVCInKingGallentBySID(SID);
            return agvc;
        }

        public override clsHostExecuting Run(IAGVS agvs, IAGVSExecutingState OrderObjectFromAgvs)
        {
            KingGallentAGVS.clsHostExcutingState excutingState = (KingGallentAGVS.clsHostExcutingState)OrderObjectFromAgvs;
            _taskObj = excutingState.executingObject;
            _headerData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(_taskObj["Header"].ToString());

            return base.Run(agvs, OrderObjectFromAgvs);
        }
    }
    internal class GpmExcutingPreProcessor : AgvsExcutingPreProcessorBase
    {
        public override IAGVC FindAGV()
        {
            return AGVCManager.FindAGVCInGPM();
        }
        public override clsLATOrderDetail OrderConvertToLATFormat()
        {
            return OrderConverter.AGVSToLAT.GPMOrderToLATOrder(aGVSExecutingState);
        }
    }
}