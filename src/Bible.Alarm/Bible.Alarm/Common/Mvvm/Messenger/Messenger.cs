using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Common.Mvvm
{
    public enum Messages
    {
        Initialized,
        ShowAlarmModal,
        HideAlarmModal,
        ShowMediaProgessModal,
        HideMediaProgressModal,
        MediaProgress
    }

    public static class Messenger<T>
    {
        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        private static Dictionary<Messages, List<Func<T, Task>>> subscribers = new Dictionary<Messages, List<Func<T, Task>>>();
        private static Dictionary<Messages, T> cache = new Dictionary<Messages, T>();
        public async static Task Publish(Messages stream, T @object = default)
        {
            if (subscribers.ContainsKey(stream))
            {
                var listeners = subscribers[stream];

                foreach (var listener in listeners.ToArray())
                {
                    await listener(@object);
                }
            }

            cache[stream] = @object;
        }

        public static void Subscribe(Messages stream, Func<T, Task> action, bool getMostRecentEvent = false)
        {
            if (subscribers.ContainsKey(stream))
            {
                subscribers[stream].Add(action);
            }
            else
            {
                subscribers[stream] = new List<Func<T, Task>>(new[] { action });
            }

            if (getMostRecentEvent && cache.ContainsKey(stream))
            {
                var @object = cache[stream];
                action(@object);
            }


        }
    }

}
