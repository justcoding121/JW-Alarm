using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bible.Alarm.Uwp
{
    public class IocSetup
    {
        private static ConcurrentDictionary<string, IContainer> containers
            = new ConcurrentDictionary<string, IContainer>();
        public static Tuple<IContainer, bool> Initialize(string containerName,
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

            var context = new Dictionary<string, object>
            {
                { "IsUwpService", isService }
            };

            IContainer container;

            if (containerName != null)
            {
                container = containers.GetOrAdd(containerName, new Container(context));
            }
            else
            {
                container = new Container(context);
            }

            UI.IocSetup.Initialize(container, isService);
            Alarm.Services.IocSetup.Initialize(container, isService);
            Alarm.Services.Uwp.IocSetup.Initialize(container, isService);
            ViewModels.IocSetup.Initialize(container, isService);

            return new Tuple<IContainer, bool>(container, true);
        }

        public static IContainer GetContainer(string containerName)
        {
            if (containers.TryGetValue(containerName, out var @value))
            {
                return value;
            }

            return null;
        }

        public bool Remove(string containerName)
        {
            return containers.TryRemove(containerName, out _);
        }

    }
}