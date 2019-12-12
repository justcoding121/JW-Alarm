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
using Bible.Alarm.Contracts.Platform;

namespace Bible.Alarm.Droid.Services.Platform
{
    public class VersionFinder : IVersionFinder
    {
        private readonly static Lazy<string> version = new Lazy<string>(() => getVersionName());
        public static VersionFinder Default => new VersionFinder();

        public string GetVersionName()
        {
            return version.Value;
        }

        private static string getVersionName()
        {
            return "Android " + Application.Context.ApplicationContext.PackageManager
                  .GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;
        }

    }
}