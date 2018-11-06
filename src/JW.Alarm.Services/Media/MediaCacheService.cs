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
        private IMediaPlayService mediaPlayService;

        public MediaCacheService(IStorageService storageService,
            DownloadService downloadService, IMediaPlayService mediaPlayService)
        {
            this.storageService = storageService;
            this.downloadService = downloadService;
            this.mediaPlayService = mediaPlayService;

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

        public async Task SetupAlarmCache(int alarmScheduleId)
        {
            var items = await mediaPlayService.ItemsToPlay(alarmScheduleId, TimeSpan.FromMinutes(15));

            foreach (var item in items)
            {
                if (!await Exists(item.Url))
                {
                    var bytes = await downloadService.DownloadAsync(item.Url);
                    await storageService.SaveFile(cacheRoot, GetCacheKey(item.Url), bytes);
                }
            }
        }

        public string GetCacheUrl(string url)
        {
            return Path.Combine(cacheRoot, GetCacheKey(url));
        }
    }
}
