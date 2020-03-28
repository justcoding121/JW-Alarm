using Android.App;
using Android.Content;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bible.Alarm.Droid
{
    public class IocSetup
    {
        private static ConcurrentDictionary<string, IContainer> containers
            = new ConcurrentDictionary<string, IContainer>();
        public static Tuple<IContainer, bool> Initialize(
            Context androidContext,
            bool isService)
        {
            var result = InitializeWithContainerName(null, androidContext, isService);
            var containerCreated = result.Item2;
            if (containerCreated)
            {
                var application = (Application)androidContext.ApplicationContext;
                Xamarin.Essentials.Platform.Init(application);
            }

            return result;
        }

        public static Tuple<IContainer, bool> InitializeWithContainerName(
            string containerName,
            Context androidContext,
            bool isService)
        {
            if (containerName != null)
            {
                containers.TryGetValue(containerName, out var existing);

                if (existing != null)
                {
                    return new Tuple<IContainer, bool>(existing, false);
                }
            }

            var context = new Dictionary<string, object>();

            context.Add("AndroidContext", androidContext);
            context.Add("IsAndroidService", isService);

            IContainer container = new Container(context);

            UI.IocSetup.Initialize(container, isService);
            Alarm.Services.IocSetup.Initialize(container, isService);
            Alarm.Services.Droid.IocSetup.Initialize(container, isService);
            ViewModels.IocSetup.Initialize(container, isService);

            if (containerName != null)
            {
                container = containers.GetOrAdd(containerName, container);
            }

            return new Tuple<IContainer, bool>(container, true);
        }

    }
}