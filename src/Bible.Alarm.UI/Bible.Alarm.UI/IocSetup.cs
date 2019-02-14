using JW.Alarm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.UI
{
    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            Container = container;
        }
    }
}
