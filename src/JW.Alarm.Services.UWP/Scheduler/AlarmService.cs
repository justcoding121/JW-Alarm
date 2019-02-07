using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Uwp
{
    public class UwpAlarmService : IAlarmService
    {
        private INotificationService notificationService;
        private IPlaylistService playlistService;
        private IMediaCacheService mediaCacheService;
        private IScheduleRepository scheduleRepository;

        public UwpAlarmService(INotificationService notificationService,
            IPlaylistService mediaPlayService,
            IMediaCacheService mediaCacheService,
            IScheduleRepository scheduleRepository)
        {
            this.notificationService = notificationService;
            this.playlistService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
            this.scheduleRepository = scheduleRepository;
        }

        public async Task Create(AlarmSchedule schedule)
        {
            var nextTrack = await playlistService.NextTrack(schedule.Id);
            nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
            scheduleNotification(schedule, nextTrack);
            await Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public async Task Update(AlarmSchedule schedule)
        {
            removeNotification(schedule.Id);

            if (schedule.IsEnabled)
            {
                var nextTrack = await playlistService.NextTrack(schedule.Id);
                nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
                scheduleNotification(schedule, nextTrack);
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
        }

        public async Task Snooze(long scheduleId)
        {
            var nextTrack = await playlistService.NextTrack(scheduleId);
            var schedule = await scheduleRepository.Read(scheduleId);

            nextTrack.PlayDetail.NotificationTime = DateTimeOffset.Now.AddMinutes(schedule.SnoozeMinutes);

            scheduleNotification(schedule, nextTrack);
        }

        public void Delete(long scheduleId)
        {
            removeNotification(scheduleId);
        }

        private void scheduleNotification(AlarmSchedule schedule, PlayItem currentTrack)
        {
            var fireDate = currentTrack.PlayDetail.NotificationTime;

            notificationService.Add(currentTrack.PlayDetail,
                schedule.Name, schedule.Name);

        }

        private void removeNotification(long scheduleId)
        {
            notificationService.Remove(scheduleId);
        }
    }
}
