using Android.Content;
using Bible.Alarm.Contracts.Battery;

namespace Bible.Alarm.Droid.Services.Battery
{
    public class BatteryOptimizationManager : IBatteryOptimizationManager
    {
        public IContainer container { get; set; }

        public BatteryOptimizationManager(IContainer container)
        {
            this.container = container;
        }

        public void ShowBatteryOptimizationExclusionSettingsPage()
        {
            Intent intent = new Intent();

            intent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            container.AndroidContext().StartActivity(intent);
        }

        public void Dispose()
        {

        }

    }
}