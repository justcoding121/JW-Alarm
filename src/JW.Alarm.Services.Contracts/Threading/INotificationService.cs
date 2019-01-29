using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface INotificationService
    {
        Task Add(string groupId, NotificationDetail detail, string title, string body, string audioUrl);

        void AddSilent(string groupId, DateTimeOffset notificationTime);

        Task Remove(long scheduleId);
        bool IsScheduled(long scheduleId);


        string GetBibleNotificationDetail(long scheduleId, BibleReadingSchedule bibleReadingSchedule);
        string GetMusicNotificationDetail(long scheduleId, AlarmMusic alarmMusicSchedule);
        Task<NotificationDetail> ParseNotificationDetail(string key);
    }
}
