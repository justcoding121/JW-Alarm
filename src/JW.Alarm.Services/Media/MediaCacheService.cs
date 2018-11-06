using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class MediaCacheService : IMediaCacheService
    {
        private readonly string cacheRoot;

        private IStorageService storageService;
        private DownloadService downloadService;
        public MediaCacheService(IStorageService storageService, 
            DownloadService downloadService)
        {
            this.storageService = storageService;
            this.downloadService = downloadService;
            cacheRoot = Path.Combine(storageService.StorageRoot, "MediaCache");
        }

        public string GetCacheKey(string url)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(url);
            return Convert.ToBase64String(plainTextBytes);
        }

        public async Task<bool> Exists(string url)
        {
            var cachePath = Path.Combine(cacheRoot, GetCacheKey(url));
            return await storageService.FileExists(cachePath);
        }

        public Task SetupAlarmCache(int alarmScheduleId)
        {
            throw new NotImplementedException();
        }

        public string GetCacheUrl(string url)
        {
            return Path.Combine(cacheRoot, GetCacheKey(url));
        }
    }
}
