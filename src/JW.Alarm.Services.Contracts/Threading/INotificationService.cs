using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Services.Contracts
{
    public interface INotificationService
    {
        void Add(int scheduleId, string detail, DateTimeOffset notificationTime, string title, string body, string audioUrl);

        bool Remove(int scheduleId);
        bool IsScheduled(int scheduleId);
        void Clear();

        string GetBibleNotificationDetail(int scheduleId, BibleReadingSchedule bibleReadingSchedule);
        string GetMusicNotificationDetail(int scheduleId, AlarmMusic alarmMusicSchedule);
        PlayDetail ParseNotificationDetail(string detail);
    }
}
