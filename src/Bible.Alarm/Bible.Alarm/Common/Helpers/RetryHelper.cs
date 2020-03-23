using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Common.Helpers
{
    public static class RetryHelper
    {
        public static async Task<T> Retry<T>(Func<Task<T>> func, int retryCount)
        {
            var delay = 1000;

            try
            {
                while (true)
                {
                    try
                    {
                        return await func();
                    }
                    catch when (retryCount-- > 0)
                    {
                        await Task.Delay(delay);
                        delay *= 2;
                    }
                }
            }
            catch
            {
                return default(T);
            }
        }
    }
}
