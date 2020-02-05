using System;

namespace Bible.Alarm.Services.Contracts
{
    public interface INotificationService : IDisposable
    {
        void ShowNotification(long scheduleId);
        void ScheduleNotification(long scheduleId, DateTimeOffset time, string title, string body);
        void Remove(long scheduleId);
        bool IsScheduled(long scheduleId);

        void ClearAll();
    }
}
