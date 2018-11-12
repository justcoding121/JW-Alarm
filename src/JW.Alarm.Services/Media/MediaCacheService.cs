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
        private IPlaylistService mediaPlayService;

        public MediaCacheService(IStorageService storageService,
            DownloadService downloadService, IPlaylistService mediaPlayService)
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

        public async Task SetupAlarmCache(long alarmScheduleId)
        {
            var playlist = await mediaPlayService.Playlist(alarmScheduleId, TimeSpan.FromMinutes(15));

            foreach (var playItem in playlist)
            {
                if (!await Exists(playItem.Url))
                {
                    var bytes = await downloadService.DownloadAsync(playItem.Url);
                    await storageService.SaveFile(cacheRoot, GetCacheKey(playItem.Url), bytes);
                }
            }
        }

        public string GetCacheUrl(string url)
        {
            return Path.Combine(cacheRoot, GetCacheKey(url));
        }
    }
}
