using Android.Widget;
using Bible.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid
{
    public class DroidToastService : ToastService
    {
        public override Task ShowMessage(string message, int seconds)
        {
            var context = IocSetup.Context;

            if(seconds <= 3)
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
