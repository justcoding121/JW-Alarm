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

namespace Bible.Alarm.Droid
{
    public static class ContainerExtension
    {
        public static Context AndroidContext(this IContainer container)
        {
            if(container.Context == null)
            {
                return null;
            }

            if (container.Context.ContainsKey("AndroidContext"))
            {
                return container.Context["AndroidContext"] as Context;
            }

            return null;
        }

        public static bool IsAndroidService(this IContainer container)
        {
            if (container.Context == null)
            {
                return false;
            }

            if (container.Context.ContainsKey("IsAndroidService"))
            {
                return (bool)container.Context["IsAndroidService"];
            }

            return false;
        }
    }
}