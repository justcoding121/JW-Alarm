using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Windows.ApplicationModel.Background;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class SchedulerTask 
    {
        private ScheduleDbContext scheduleDbContext;
        private IMediaCacheService mediaCacheService;
        public SchedulerTask(ScheduleDbContext scheduleDbContext, IMediaCacheService mediaCacheService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.mediaCacheService = mediaCacheService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var schedules = await scheduleDbContext.AlarmSchedules.ToListAsync();

            foreach (var schedule in schedules)
            {
                //var nextFire = schedule.NextFireDate();
            }

            deferral.Complete();
        }
    }
}
