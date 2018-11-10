using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface INotificationService
    {
        Task Add(int scheduleId, PlayDetail detail, DateTimeOffset notificationTime, string title, string body, string audioUrl);

        Task Remove(int scheduleId);
        bool IsScheduled(int scheduleId);


        string GetBibleNotificationDetail(int scheduleId, BibleReadingSchedule bibleReadingSchedule);
        string GetMusicNotificationDetail(int scheduleId, AlarmMusic alarmMusicSchedule);
        Task<PlayDetail> ParseNotificationDetail(string key);
    }
}
