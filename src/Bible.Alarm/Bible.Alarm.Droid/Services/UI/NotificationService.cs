using Android.App;
using Android.Content;
using Bible.Alarm.Droid;
using Bible.Alarm.Droid.Services.Handlers;
using Bible.Alarm.Droid.Services.Tasks;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Tasks;
using MediaManager;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid
{
    public class DroidNotificationService : INotificationService
    {
        private readonly IContainer container;

        public DroidNotificationService(IContainer container)
        {
            this.container = container;
        }

        public async void ShowNotification(long scheduleId)
        {
            //AlarmSetupService.ShowNotification(container.AndroidContext(), scheduleId);
            
            var alarmHandler = container.Resolve<AndroidAlarmHandler>();
            await alarmHandler.Handle(scheduleId, true);
        }

        public Task ScheduleNotification(long scheduleId, DaysOfWeek daysOfWeek, DateTimeOffset time,
            string title, string body)
        {
            if (container.IsAndroidService())
            {
                AlarmSetupService.ScheduleNotification(container.AndroidContext(), scheduleId, time, title, body);
            }
            else
            {
                Intent intent = new Intent(container.AndroidContext(), typeof(AlarmSetupService));
                intent.PutExtra("Action", "Add");
                intent.PutExtra("ScheduleId", scheduleId.ToString());
                intent.PutExtra("Time", time.ToString());
                intent.PutExtra("Title", title);
                intent.PutExtra("Body", body);
                container.AndroidContext().StartService(intent);
            }

            return Task.CompletedTask;
        }

        public Task Remove(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);

            if (pIntent != null)
            {
                var alarmManager = (AlarmManager)container.AndroidContext().GetSystemService(Context.AlarmService);
                alarmManager?.Cancel(pIntent);
                pIntent.Cancel();
            }

            return Task.CompletedTask;
        }

        public Task<bool> IsScheduled(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);
            return Task.FromResult(pIntent != null);
        }

        private PendingIntent findIntent(long scheduleId)
        {
            var context = container.AndroidContext();

            var alarmIntent = new Intent(context, typeof(AlarmRingerReceiver));
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    (int)scheduleId,
                    alarmIntent,
                    PendingIntentFlags.NoCreate);

            return pIntent;
        }

        public Task<bool> CanSchedule()
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {

        }
    }

}
