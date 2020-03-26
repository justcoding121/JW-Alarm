using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Services.Contracts;
using System;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private iOSAlarmHandler alarmHandler;

        public iOSNotificationService(iOSAlarmHandler alarmHandler)
        {
            this.alarmHandler = alarmHandler;
        }

        public async void ShowNotification(long scheduleId)
        {
            await alarmHandler.Handle(scheduleId);
        }

        public void ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            //throw new NotImplementedException();
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
            // throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }

}
