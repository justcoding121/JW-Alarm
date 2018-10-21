using JW.Alarm.Services.Contracts;
using Windows.ApplicationModel.Background;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class SchedulerTask 
    {
        private IAlarmScheduleService alarmService;
        public SchedulerTask(IAlarmScheduleService alarmService)
        {
            this.alarmService = alarmService;
        }

        public void Handle(IBackgroundTaskInstance backgroundTask)
        {

        }
    }
}
