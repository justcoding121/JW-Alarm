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
            var alarmIntent = new Intent(context, typeof(AlarmTask));

            alarmIntent.SetAction(buildActionName(scheduleId.ToString()));
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    0,
                    alarmIntent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            // Figure out the alaram in milliseconds.
            var utcTime = time.UtcDateTime;
            var epochDif = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            var notifyTimeInInMilliseconds = utcTime.AddSeconds(-epochDif).Ticks / 10000;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                alarmService.Set(AlarmType.RtcWakeup, notifyTimeInInMilliseconds, pIntent);
            }
            else if(Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                alarmService.SetExact(AlarmType.RtcWakeup, notifyTimeInInMilliseconds, pIntent);
            }
            else
            {
                alarmService.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, notifyTimeInInMilliseconds, pIntent);
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

            var alarmIntent = new Intent(context, typeof(AlarmTask));
            alarmIntent.SetAction(buildActionName(scheduleId.ToString()));

            var pIntent = PendingIntent.GetBroadcast(
                  context,
                  0,
                  alarmIntent,
                  PendingIntentFlags.NoCreate);

            return pIntent;
        }
        /// <summary>
        ///     The action to append end of the action name.
        /// </summary>
        private const string ActionSuffix = "NOTIFICATION";

        /// <summary>
        ///     Builds the action name for the notification intent.
        /// </summary>
        /// <param name="notificationId">The unique ID of the notification.</param>
        /// <returns>The action name with the unique notification ID build into it.</returns>
        /// <remarks>
        ///     The action name looks something like:
        ///     <example>
        ///         com.saturdaymp.exampleclient.NOTIFICATIONS-[notificationId]
        ///     </example>
        /// </remarks>
        internal static string buildActionName(string notificationId)
        {
            return Application.Context.PackageName + "." + ActionSuffix + "-" + notificationId;
        }

    }

}
