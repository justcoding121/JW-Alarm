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
        public override Task ShowMessage(string message, int seconds)
        {
            if (clearRequest != null)
            {
                return Task.CompletedTask;
            }

            Task.Delay(0)
                 .ContinueWith(async (x) =>
                     await showAlert(message, (double)seconds), taskScheduler);

            return Task.CompletedTask;
        }

        private async Task showAlert(string message, double seconds)
        {
            clearRequest = new TaskCompletionSource<bool>();
            await @lock.WaitAsync();
         
            try
            {
            
                var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

                await Task.WhenAny(clearRequest.Task, Task.Delay((int)(seconds * 1000))).ConfigureAwait(true);
                alert.DismissViewController(true, null);
               
            }
            finally
            {
                @lock.Release();
                clearRequest = null;
            }
        }

        private static TaskCompletionSource<bool> clearRequest;
        public override Task Clear()
        {
            if (clearRequest != null)
            {
                clearRequest.SetResult(true);
            }

            return Task.CompletedTask;
        }
    }
}
