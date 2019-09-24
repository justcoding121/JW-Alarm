﻿using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Bible.Alarm.UITests
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp.Android.InstalledApp("com.jthomas.info.Bible.Alarm").StartApp();
            }

            return ConfigureApp.iOS.StartApp();
        }
    }
}