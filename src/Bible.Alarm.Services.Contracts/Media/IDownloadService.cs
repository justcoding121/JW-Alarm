using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IDownloadService
    {
        Task<byte[]> DownloadAsync(string url, string alternativeUrl = null);
    }
}
