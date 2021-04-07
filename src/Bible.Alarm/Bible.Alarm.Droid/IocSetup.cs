using Android.App;
using Android.Content;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bible.Alarm.Droid
{
    public class IocSetup
    {
        private static ConcurrentDictionary<object, IContainer> containers
            = new();

        public static void Remove(object key)
        {
            containers.TryRemove(key, out var _);
        }

        public static Tuple<IContainer, bool> Initialize(
            Context androidContext,
            bool isService)
        {
            if (containers.TryGetValue(androidContext, out var existing))
            {
                return new Tuple<IContainer, bool>(existing, false);
            }

            var context = new Dictionary<string, object>
            {
                { "AndroidContext", androidContext },
                { "IsAndroidService", isService }
            };

            IContainer container = new Container(context);

            UI.IocSetup.Initialize(container, isService);
            Alarm.Services.IocSetup.Initialize(container, isService);
            Alarm.Services.Droid.IocSetup.Initialize(container, isService);
            ViewModels.IocSetup.Initialize(container, isService);

            container = containers.GetOrAdd(androidContext, container);

            return new Tuple<IContainer, bool>(container, true);
        }

    }
}