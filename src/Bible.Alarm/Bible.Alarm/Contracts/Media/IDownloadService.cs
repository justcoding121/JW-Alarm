using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IDownloadService : IDisposable
    {
        Task<byte[]> DownloadAsync(string url, string alternativeUrl = null);
    }
}
