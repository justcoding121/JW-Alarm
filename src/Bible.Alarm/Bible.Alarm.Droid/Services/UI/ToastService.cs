using Android.Widget;
using Bible.Alarm.Droid;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid
{
    public class DroidToastService : ToastService, IDisposable
    {
        private readonly IContainer container;
        private static readonly SemaphoreSlim @lock = new SemaphoreSlim(1);
        private static Toast latest;
        public DroidToastService(IContainer container)
        {
            this.container = container;
        }
        public async override Task ShowMessage(string message, int seconds)
        {
            await @lock.WaitAsync();

            try
            {
                var context = container.AndroidContext();

                if (seconds <= 3)
                {
                    latest = Toast.MakeText(context, message, ToastLength.Short);
                }
                else
                {
                    latest = Toast.MakeText(context, message, ToastLength.Long);
                }

                latest.Show();

            }
            finally
            {
                @lock.Release();
            }
        }

        //not needed for android
        public async override Task Clear()
        {
            await @lock.WaitAsync();

            try
            {
                latest?.Cancel();
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}
