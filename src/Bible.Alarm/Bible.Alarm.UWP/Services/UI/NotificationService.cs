using Bible.Alarm.Services.Contracts;
using NLog;
using System;
using System.Threading.Tasks;
using Bible.Alarm.UWP.Services.Handlers;

namespace Bible.Alarm.Services.UWP
{
    public class UwpNotificationService : INotificationService
    {
        private readonly IContainer container;

        public UwpNotificationService(IContainer container)
        {
            this.container = container;
        }

        public void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<UwpAlarmHandler>();
            _ = iosAlarmHandler.Handle(scheduleId);
        }

        public Task ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            return Task.CompletedTask;

        }

        public Task Remove(long scheduleId)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsScheduled(long scheduleId)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {

        }
    }

}
