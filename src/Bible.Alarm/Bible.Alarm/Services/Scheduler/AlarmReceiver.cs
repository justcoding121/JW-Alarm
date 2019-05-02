using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    public class AlarmReceiver
    {
        public event EventHandler Received;

        public void Raise(long scheduleId)
        {
            Received?.Invoke(this, new AlarmReceiverEventArgs(scheduleId));
        }

        public class AlarmReceiverEventArgs : EventArgs
        {
            public AlarmReceiverEventArgs(long scheduleId)
            {
                this.ScheduleId = scheduleId;
            }

            public long ScheduleId { get; private set; }
        }
    }

}
