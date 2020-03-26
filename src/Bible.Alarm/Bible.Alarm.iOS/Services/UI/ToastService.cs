using Bible.Alarm.Services.iOS;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSToastService))]
namespace Bible.Alarm.Services.iOS
{
    public class iOSToastService : ToastService, IDisposable
    {
        private IContainer container;
        public iOSToastService(IContainer container)
        {
            this.container = container;
        }
        public override Task ShowMessage(string message, int seconds)
        {
            showAlert(message, (double)seconds);

            return Task.CompletedTask;
        }

        NSTimer alertDelay;
        UIAlertController alert;

        private void showAlert(string message, double seconds)
        {
            alertDelay = NSTimer.CreateScheduledTimer(seconds, (obj) =>
            {
                dismissMessage();
            });

            alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void dismissMessage()
        {
            if (alert != null)
            {
                alert.DismissViewController(true, null);
            }
            if (alertDelay != null)
            {
                alertDelay.Dispose();
            }
        }
    }
}
