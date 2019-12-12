using Android.App;
using Android.Content;
using Bible.Alarm.Droid.Services.Tasks;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Tasks;
using NLog;
using System;
using Bible.Alarm.Droid;

namespace Bible.Alarm.Services.Droid
{
    public class DroidNotificationService : INotificationService
    {
        private IContainer container;

        public DroidNotificationService(IContainer container)
        {
            this.container = container;
        }

        public void ShowNotification(long scheduleId)
        {
            AlarmSetupService.ShowNotification(container.AndroidContext(), scheduleId);
        }

        public void ScheduleNotification(long scheduleId, DateTimeOffset time,
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
        }

        public void Remove(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);

            if (pIntent != null)
            {
                var alarmManager = (AlarmManager)container.AndroidContext().GetSystemService(Context.AlarmService);
                alarmManager?.Cancel(pIntent);
                pIntent.Cancel();
            }
        }

        public bool IsScheduled(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);
            return pIntent != null;
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

        public void ClearAll()
        {
            NotificationManager notificationManager = (NotificationManager)container.AndroidContext().GetSystemService(Context.NotificationService);
            notificationManager.CancelAll();
        }

        public void Dispose()
        {

        }
    }

}
