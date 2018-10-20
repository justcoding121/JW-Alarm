using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    /// <summary>
    /// Download service
    /// </summary>
    public class DownloadService
    {
        private readonly int retryAttempts = 3;

        private HttpClientHandler handler;
        public DownloadService(HttpClientHandler handler)
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
            return await Retry(async () =>
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
                    using (var client = new HttpClient(handler, false))
                    {
                        return await client.GetByteArrayAsync(alternativeUrl);
                    }
                }

            }, retryAttempts);
        }

        private static async Task<T> Retry<T>(Func<Task<T>> func, int retryCount)
        {
            var delay = 1000;

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
    }
}