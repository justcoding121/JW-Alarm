using Android.App;
using Android.Content;
using Android.OS;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Droid.Tasks;
using System;

namespace JW.Alarm.Services.Droid
{
    public class DroidNotificationService : INotificationService
    {
        IMediaCacheService mediaCacheService;

        public DroidNotificationService(IMediaCacheService mediaCacheService)
        {
            this.mediaCacheService = mediaCacheService;
        }

        public void Add(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            var context = Application.Context;
            var alarmIntent = new Intent();
            alarmIntent.SetAction(buildActionName());
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    (int)scheduleId,
                    alarmIntent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            // Figure out the alaram in milliseconds.
            var milliSecondsRemaining = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)time.Subtract(DateTimeOffset.Now).TotalSeconds * 1000;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                alarmService.Set(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
            else if(Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                alarmService.SetExact(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
            else
            {
                alarmService.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
        }


        public void Remove(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);

            if (pIntent != null)
            {
                var context = Application.Context;
                var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
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

            var pIntent = PendingIntent.GetBroadcast(
                  context,
                  (int)scheduleId,
                  alarmIntent,
                  PendingIntentFlags.NoCreate);

            return pIntent;
        }

        internal static string buildActionName()
        {
            return "com.bible.alarm.NOTIFICATION";
        }
    }

}
