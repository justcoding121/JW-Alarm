using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class AlarmService : IAlarmService
    {
        private INotificationService notificationService;
        private IPlaylistService playlistService;
        private IMediaCacheService mediaCacheService;
        private ScheduleDbContext scheduleDbContext;

        public AlarmService(INotificationService notificationService,
            IPlaylistService mediaPlayService,
            IMediaCacheService mediaCacheService,
            ScheduleDbContext scheduleDbContext)
        {
            this.notificationService = notificationService;
            this.playlistService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
            this.scheduleDbContext = scheduleDbContext;
        }

        public async Task Create(AlarmSchedule schedule, bool downloadAlarmMedia)
        {
            var nextTrack = await playlistService.NextTrack(schedule.Id);
            nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
            scheduleNotification(schedule, false);
            var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public void Update(AlarmSchedule schedule)
        {
            removeNotification(schedule.Id);

            if (schedule.IsEnabled)
            {
                scheduleNotification(schedule, false);
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
        }

        public async Task Snooze(long scheduleId)
        {
            var schedule = await scheduleDbContext.AlarmSchedules.AsNoTracking().FirstAsync(x => x.Id == scheduleId);
            scheduleNotification(schedule, true);
        }

        public void Delete(long scheduleId)
        {
            removeNotification(scheduleId);
        }

        private void scheduleNotification(AlarmSchedule schedule, bool isSnoozeNotification)
        {
            notificationService.Add(schedule.Id, isSnoozeNotification ?
                DateTimeOffset.Now.AddMinutes(schedule.SnoozeMinutes) : schedule.NextFireDate(), schedule.Name,
                schedule.MusicEnabled ? "Playing alarm music." : "Playing Bible.");
        }

        private void removeNotification(long scheduleId)
        {
            notificationService.Remove(scheduleId);
        }
    }
}
