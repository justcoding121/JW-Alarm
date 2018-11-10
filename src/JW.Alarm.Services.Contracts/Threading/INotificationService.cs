using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface INotificationService
    {
        Task Add(long scheduleId, NotificationDetail detail, DateTimeOffset notificationTime, string title, string body, string audioUrl);

        Task Remove(long scheduleId);
        bool IsScheduled(long scheduleId);


        string GetBibleNotificationDetail(long scheduleId, BibleReadingSchedule bibleReadingSchedule);
        string GetMusicNotificationDetail(long scheduleId, AlarmMusic alarmMusicSchedule);
        Task<NotificationDetail> ParseNotificationDetail(string key);
    }
}
