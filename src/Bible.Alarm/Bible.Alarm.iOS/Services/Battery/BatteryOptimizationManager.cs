using Bible.Alarm.Contracts.Battery;
using System;

namespace Bible.Alarm.iOS.Services.Battery
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
            throw new PlatformNotSupportedException();
        }

        public void Dispose()
        {

        }

    }
}