using MediaManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Common.Extensions
{
    public static class MediaManagerExtensions
    {
        public static bool IsPreparedEx(this IMediaManager mediaManager)
        {
            if (CurrentDevice.RuntimePlatform == Device.Android)
            {
                return mediaManager.IsPrepared();
            }

            return mediaManager.IsPrepared()
                && mediaManager.Position.TotalMilliseconds > 0;
        }

        public static async Task StopEx(this IMediaManager mediaManager)
        {
            await mediaManager.Stop();
        }
    }
}
