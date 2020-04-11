using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Models;
using Foundation;
using Newtonsoft.Json;
using UIKit;

namespace Bible.Alarm.iOS.Helpers
{
    public class PnsService
    {
#if DEBUG
        private static readonly string url = "http://192.168.1.64:5010/api/v1/ScheduleAlarm";
#else
        private static readonly string url = "https://production-push.jthomas.info/api/v1/ScheduleAlarm";
#endif

        public static async Task<HttpResponseMessage> RegisterDevice(string deviceId, string deviceToken)
        {
            var request = new DeviceRequest()
            {
                DeviceId = deviceId,
                DeviceToken = deviceToken
            };

            return await RetryHelper.Retry(async () =>
            {
                using var client = new HttpClient();
                var payload = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                return await client.PostAsync(url, content);

            }, 3, true);

        }

        public static async Task<HttpResponseMessage> ScheduleAlarm(string deviceId, string deviceToken,
                        DateTime alarmTime, long notificationId)
        {

            var request = new ScheduleAlarmRequest()
            {
                DeviceId = deviceId,
                DeviceToken = deviceToken,
                AlarmTime = alarmTime,
                NotificationId = notificationId
            };

            //Send to server
            return await RetryHelper.Retry(async () =>
            {
                using var client = new HttpClient();
                var payload = JsonConvert.SerializeObject(request);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                return await client.PostAsync(url, content);

            }, 3, true);

        }

        public static async Task<HttpResponseMessage> RemoveAlarm(string deviceId,
                    string deviceToken, long notificationId)
        {
            var request = new CancelAlarmRequest()
            {
                DeviceId = deviceId,
                DeviceToken = deviceToken,
                NotificationId = notificationId
            };

            //Send to server
            return await RetryHelper.Retry(async () =>
            {
                using var client = new HttpClient();
                var payload = JsonConvert.SerializeObject(request);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                return await client.PostAsync(url, content);

            }, 3, true);
        }
    }
}