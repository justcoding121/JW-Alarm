using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bible.Alarm.Contracts.Battery;
using Xamarin.Forms;

namespace Bible.Alarm.Droid.Services.Battery
{
    public class BatteryOptimizationManager : IBatteryOptimizationManager
    {
        public void ShowBatteryOptimizationExclusionSettingsPage()
        {
            Intent intent = new Intent();

            intent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            IocSetup.Context.StartActivity(intent);
        }
    }
}