using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Services.Contracts;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private readonly IContainer container;

        public iOSNotificationService(IContainer container)
        {
            this.container = container;
        }

        public void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
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
