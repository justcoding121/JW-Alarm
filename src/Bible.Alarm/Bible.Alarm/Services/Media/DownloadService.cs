using Bible.Alarm.Common.Helpers;
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
            return await RetryHelper.Retry(async () =>
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
                    if (alternativeUrl == null)
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


        public void Dispose()
        {
            handler.Dispose();
        }
    }
}