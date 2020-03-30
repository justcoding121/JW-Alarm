using Bible.Alarm.Services.iOS;
using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSToastService))]
namespace Bible.Alarm.Services.iOS
{
    public class iOSToastService : ToastService, IDisposable
    {
        private readonly TaskScheduler taskScheduler;

        public iOSToastService(TaskScheduler taskScheduler)
        {
            this.taskScheduler = taskScheduler;
        }

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);
        public async override Task ShowMessage(string message, int seconds)
        {
            await @lock.WaitAsync();

            try
            {
                await Task.Delay(0)
                    .ContinueWith(async (x) =>
                        await showAlert(message, (double)seconds), taskScheduler);

            }
            finally
            {
                @lock.Release();
            }
        }

        private async Task showAlert(string message, double seconds)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            await Task.Delay((int)(seconds * 1000)).ConfigureAwait(true);

            alert.DismissViewController(true, null);
        }
    }
}
