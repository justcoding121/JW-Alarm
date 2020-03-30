using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Common.Helpers
{
    public static class RetryHelper
    {
        public static async Task<T> Retry<T>(Func<Task<T>> func, int retryCount, bool @throw = false)
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
                if (@throw)
                {
                    throw;
                }

                return default(T);
            }
        }
    }
}
