using Android.Content;
using Bible.Alarm.Contracts.Battery;
using NLog;
using System;

namespace Bible.Alarm.Droid.Services.Battery
{
    public class BatteryOptimizationManager : IBatteryOptimizationManager
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        public IContainer container { get; set; }

        public BatteryOptimizationManager(IContainer container)
        {
            this.container = container;
        }

        public void ShowBatteryOptimizationExclusionSettingsPage()
        {
            try
            {
                Intent intent = new Intent();

                intent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
                container.AndroidContext().StartActivity(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to show batter optimization dialog.");
            }
        }

        public void Dispose()
        {

        }

    }
}