using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.AGVC.AGVCStates
{
    /// <summary>
    /// 存放四種不同等級的Alarm事件
    /// </summary>
    public class AlarmStates
    {

        public enum ALARM_TYPE
        {
            FATAL, ERROR, WARNING, NOTICES
        }



        public List<Alarm> NewestFatals { get; set; } = new List<Alarm>();
        public List<Alarm> NewestErrors { get; set; } = new List<Alarm>();
        public List<Alarm> NewestWarnings { get; set; } = new List<Alarm>();
        public List<Alarm> NewestNotices { get; set; } = new List<Alarm>();


        public List<Alarm> Fatals { get; set; } = new List<Alarm>();
        public List<Alarm> Errors { get; set; } = new List<Alarm>();
        public List<Alarm> Warnings { get; set; } = new List<Alarm>();
        public List<Alarm> Notices { get; set; } = new List<Alarm>();


        public void Update(AlarmStates newStates)
        {

            NewestFatals = newStates.Fatals;
            NewestErrors = newStates.Errors;
            NewestWarnings = newStates.Warnings;
            NewestNotices = newStates.Notices;



            var newFatals = newStates.Fatals?.FindAll(fatal => !Fatals.Any(f => f.time == fatal.time));
            var newErrors = newStates.Errors?.FindAll(fatal => !Errors.Any(f => f.time == fatal.time));
            var newWarnings = newStates.Warnings?.FindAll(fatal => !Warnings.Any(f => f.time == fatal.time));
            var newNotices = newStates.Notices?.FindAll(fatal => !Notices.Any(f => f.time == fatal.time));


            if (newFatals != null)
                Fatals.AddRange(newFatals);
            if (newErrors != null)
                Errors.AddRange(newErrors);
            if (newWarnings != null)
                Warnings.AddRange(newWarnings);
            if (newNotices != null)
                Notices.AddRange(newNotices);
        }


        public class Alarm
        {
            public DateTime time { get; set; }
            public string code { get; set; }
            public string description { get; set; }

        }

    }

}
