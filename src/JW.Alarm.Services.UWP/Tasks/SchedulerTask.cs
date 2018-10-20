using JW.Alarm.Services.Contracts;
using Windows.ApplicationModel.Background;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class SchedulerTask 
    {
        private IScheduleService alarmService;
        public SchedulerTask(IScheduleService alarmService)
        {
            this.alarmService = alarmService;
        }

        public void Handle(IBackgroundTaskInstance backgroundTask)
        {

        }
    }
}
