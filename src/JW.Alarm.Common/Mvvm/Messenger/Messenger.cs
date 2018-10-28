using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JW.Alarm.Common.Mvvm.Messenger
{
    public static class Messenger
    {
        private static Dictionary<string, List<Func<object, Task>>> subscribers = new Dictionary<string, List<Func<object, Task>>>();

        public async static Task Publish<T>(T @object) where T : class
        {
            if (subscribers.ContainsKey(typeof(T).Name))
            {
                var listeners = subscribers[typeof(T).Name];

                foreach (var listener in listeners)
                {
                    await listener(@object);
                }
            }
        }

        public static void Subscribe<T>(Func<object, Task> action) where T : class
        {
            if (subscribers.ContainsKey(typeof(T).Name))
            {
                subscribers[typeof(T).Name].Add(action);
            }
            else
            {
                subscribers[typeof(T).Name] = new List<Func<object, Task>>(new[] { action });
            }

        }
    }

}
