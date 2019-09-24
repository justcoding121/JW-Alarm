using Bible.Alarm.Services.Contracts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    /// <summary>
    /// Download service
    /// </summary>
    public class DownloadService : IDownloadService
    {
        private readonly int retryAttempts = 3;

        private HttpMessageHandler handler;
        public DownloadService(HttpMessageHandler handler)
        {
            this.handler = handler;
        }

        /// <summary>
        /// Dowload the file from the Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<byte[]> DownloadAsync(string url, string alternativeUrl = null)
        {
            return await retry(async () =>
            {
                try
                {
                    using (var client = new HttpClient(handler, false))
                    {
                        return await client.GetByteArrayAsync(url);
                    }
                }
                catch
                {
                    if(alternativeUrl == null)
                    {
                        throw;
                    }

                    using (var client = new HttpClient(handler, false))
                    {
                        return await client.GetByteArrayAsync(alternativeUrl);
                    }
                }

            }, retryAttempts);
        }

        private static async Task<T> retry<T>(Func<Task<T>> func, int retryCount)
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