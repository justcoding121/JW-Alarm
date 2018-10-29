using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JW.Alarm.Common.Mvvm
{
    public enum Messages
    {
        Progress
    }

    public static class Messenger<T>
    {
        private static Dictionary<Messages, List<Func<T, Task>>> subscribers = new Dictionary<Messages, List<Func<T, Task>>>();

        public async static Task Publish(Messages stream, T @object)
        {
            if (subscribers.ContainsKey(stream))
            {
                var listeners = subscribers[stream];

                foreach (var listener in listeners)
                {
                    await listener(@object);
                }
            }
        }

        public static void Subscribe(Messages stream, Func<T, Task> action)
        {
            if (subscribers.ContainsKey(stream))
            {
                subscribers[stream].Add(action);
            }
            else
            {
                subscribers[stream] = new List<Func<T, Task>>(new[] { action });
            }

        }
    }

}
