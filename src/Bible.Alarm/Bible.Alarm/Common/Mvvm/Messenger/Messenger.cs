using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Common.Mvvm
{
    public enum MvvmMessages
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

        private static Dictionary<MvvmMessages, List<Func<T, Task>>> subscribers = new Dictionary<MvvmMessages, List<Func<T, Task>>>();
        private static Dictionary<MvvmMessages, T> cache = new Dictionary<MvvmMessages, T>();
        public async static Task Publish(MvvmMessages stream, T @object = default)
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

        public static void Subscribe(MvvmMessages stream, Func<T, Task> action, bool getMostRecentEvent = false)
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
