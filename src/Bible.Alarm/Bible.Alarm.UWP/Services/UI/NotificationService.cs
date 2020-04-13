using Bible.Alarm;
using Bible.Alarm.Services.Contracts;
using System;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP
{
    public class UwpNotificationService : INotificationService
    {
        private readonly IContainer container;
        public UwpNotificationService(IContainer container)
        {
            this.container = container;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsScheduled(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public Task Remove(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public Task ScheduleNotification(long scheduleId, DateTimeOffset time, string title, string body)
        {
            throw new NotImplementedException();
        }

        public void ShowNotification(long scheduleId)
        {
            throw new NotImplementedException();
        }
    }

}
