using JW.Alarm.Services.Contracts;
using Windows.ApplicationModel.Background;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class SchedulerTask 
    {
        private IAlarmScheduleService alarmService;
        private IMediaCacheService mediaCacheService;
        public SchedulerTask(IAlarmScheduleService alarmService, IMediaCacheService mediaCacheService)
        {
            this.alarmService = alarmService;
            this.mediaCacheService = mediaCacheService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var schedules = await alarmService.AlarmSchedules;

            foreach(var schedule in schedules)
            {
                var nextFire = schedule.Value.NextFireDate();
            }

            deferral.Complete();
        }
    }
}
