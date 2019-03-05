using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
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
        private IDownloadService downloadService;
        private IPlaylistService mediaPlayService;
        private ScheduleDbContext dbContext;

        public MediaCacheService(IStorageService storageService,
            IDownloadService downloadService, IPlaylistService mediaPlayService,
            ScheduleDbContext dbContext)
        {
            this.storageService = storageService;
            this.downloadService = downloadService;
            this.mediaPlayService = mediaPlayService;
            this.dbContext = dbContext;

            cacheRoot = Path.Combine(storageService.StorageRoot, "MediaCache");
        }

        public string GetCacheFileName(string url)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(url);
            return Convert.ToBase64String(plainTextBytes) + ".mp3";
        }

        public string GetCacheFilePath(string url)
        {
            return Path.Combine(cacheRoot, GetCacheFileName(url));
        }

        public async Task<bool> Exists(string url)
        {
            var cachePath = Path.Combine(cacheRoot, GetCacheFileName(url));
            return await storageService.FileExists(cachePath);
        }

        public async Task SetupAlarmCache(long alarmScheduleId)
        {
            var playlist = await mediaPlayService.NextTracks(alarmScheduleId, TimeSpan.FromMinutes(15));

            foreach (var playItem in playlist)
            {
                if (!await Exists(playItem.Url))
                {
                    var bytes = await downloadService.DownloadAsync(playItem.Url);
                    await storageService.SaveFile(cacheRoot, GetCacheFileName(playItem.Url), bytes);
                }
            }
        }

        public async Task CleanUp()
        {
            var schedules = await dbContext.AlarmSchedules.ToListAsync();
            var files = (await storageService.GetAllFiles(cacheRoot)).ToDictionary(x => x, null);

            foreach (var schedule in schedules)
            {
                var playlist = await mediaPlayService.NextTracks(schedule.Id, TimeSpan.FromMinutes(15));
                var fileNames = playlist.Select(x => GetCacheFilePath(x.Url)).ToList();

                fileNames.ForEach(x =>
                {
                    if (files.ContainsKey(x))
                    {
                        files.Remove(x);
                    }
                });
            }

            files.Select(x => x.Key).ToList().ForEach(x =>
            {
                try { storageService.DeleteFile(x); }
                catch { }
            });
        }
    }
}
