using Bible.Alarm.Services.Contracts;
using System;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private IContainer container;

        public iOSNotificationService(IContainer container)
        {
            this.container = container;
        }

        public void ShowNotification(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public void ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            throw new NotImplementedException();
        }

        public void Remove(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public bool IsScheduled(long scheduleId)
        {
            throw new NotImplementedException();
        }


        public void ClearAll()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }

}
