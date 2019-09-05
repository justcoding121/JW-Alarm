using Android.Widget;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid
{
    public class DroidPopUpService : ToastService
    {
        public override Task ShowMessage(string message, int seconds)
        {
            var context = IocSetup.Context;
            var tostMessage = message;
            Toast.MakeText(context, message, ToastLength.Short).Show();

            return Task.CompletedTask;
        }
    }
}
