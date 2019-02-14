﻿using JW.Alarm;

namespace Bible.Alarm.UI.UWP
{
 
    public static class IocSetup
    {
        public static IContainer Container;
        public static void Initialize()
        {
            var container = JW.Alarm.Container.Default;

            Bible.Alarm.UI.IocSetup.Initialize(container);
            JW.Alarm.Services.IocSetup.Initialize(container);
            JW.Alarm.Services.Uwp.IocSetup.Initialize(container);
            JW.Alarm.ViewModels.IocSetup.Initialize(container);

            Container = container;
        }


    }
}