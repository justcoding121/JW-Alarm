using Android.App;
using Android.Content;
using Android.OS;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Droid.Tasks;
using Microsoft.AppCenter.Analytics;
using System;

namespace JW.Alarm.Services.Droid
{
    public class DroidNotificationService : INotificationService
    {
        public void Add(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            Analytics.TrackEvent($"Scheduling at {DateTime.Now}");

            Intent intent = new Intent(Application.Context, typeof(AlarmSetupTask));
            intent.PutExtra("Action", "Add");
            intent.PutExtra("ScheduleId", scheduleId.ToString());
            intent.PutExtra("Time", time.ToString());
            intent.PutExtra("Title", title);
            intent.PutExtra("Body", body);
            Application.Context.StartService(intent);

            Analytics.TrackEvent($"Scheduling Start Service at {DateTime.Now}");
        }


        public void Remove(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);

            if (pIntent != null)
            {
                var context = this;
                var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
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
            var context = Application.Context;

            var alarmIntent = new Intent();
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());
            alarmIntent.SetAction(buildActionName());

            var pIntent = PendingIntent.GetService(
                  context,
                  (int)scheduleId,
                  alarmIntent,
                  PendingIntentFlags.NoCreate);

            return pIntent;
        }

        internal static string buildActionName()
        {
            return "com.bible.alarm.RING";
        }
    }

}
