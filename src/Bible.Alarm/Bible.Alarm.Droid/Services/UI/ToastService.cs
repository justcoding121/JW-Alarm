using Android.Widget;
using Bible.Alarm.Droid;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid
{
    public class DroidToastService : ToastService, IDisposable
    {
        private readonly IContainer container;
        public DroidToastService(IContainer container)
        {
            this.container = container;
        }
        public override Task ShowMessage(string message, int seconds)
        {
            var context = container.AndroidContext();

            if (seconds <= 3)
            {
                Toast.MakeText(context, message, ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(context, message, ToastLength.Long).Show();
            }


            return Task.CompletedTask;
        }
    }
}
