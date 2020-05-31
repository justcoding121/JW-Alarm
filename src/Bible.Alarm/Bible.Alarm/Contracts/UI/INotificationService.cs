using Bible.Alarm.Models;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface INotificationService : IDisposable
    {
        void ShowNotification(long scheduleId);
        Task ScheduleNotification(long scheduleId, DaysOfWeek daysOfWeek, DateTimeOffset time,  string title, string body);
        Task Remove(long scheduleId);
        Task<bool> IsScheduled(long scheduleId);

        Task<bool> CanSchedule();
    }
}
