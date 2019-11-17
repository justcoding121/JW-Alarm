using Android.Content;
using Bible.Alarm.Contracts.Battery;

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

        public void Dispose()
        {

        }

    }
}