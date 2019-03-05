using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
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
        private ScheduleDbContext scheduleDbContext;
        private MediaService mediaService;

        public MediaCacheService(IStorageService storageService,
            IDownloadService downloadService, IPlaylistService mediaPlayService,
            ScheduleDbContext dbContext, MediaService mediaService)
        {
            this.storageService = storageService;
            this.downloadService = downloadService;
            this.mediaPlayService = mediaPlayService;
            this.scheduleDbContext = dbContext;
            this.mediaService = mediaService;

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
                    byte[] bytes = null;

                    try
                    {
                        bytes = await downloadService.DownloadAsync(playItem.Url);
                    }
                    catch
                    {
                        var playDetail = playItem.PlayDetail;

                        string url;

                        if (playDetail.PlayType == Models.PlayType.Bible)
                        {
                            url = await getBibleChapterUrl(playDetail.LanguageCode, playDetail.PublicationCode, playDetail.BookNumber, playDetail.ChapterNumber);
                            await mediaService.UpdateBibleTrackUrl(playDetail.LanguageCode, playDetail.PublicationCode, playDetail.BookNumber, playDetail.ChapterNumber, url);
                        }
                        else
                        {
                            url = await getMusicTrackUrl(playDetail.LanguageCode, playDetail.PublicationCode, playDetail.TrackNumber);

                            if (playDetail.LanguageCode == null)
                            {
                                await mediaService.UpdateMelodyTrackUrl(playDetail.PublicationCode, playDetail.TrackNumber, url);
                            }
                            else
                            {
                                await mediaService.UpdateVocalTrackUrl(playDetail.LanguageCode, playDetail.PublicationCode, playDetail.TrackNumber, url);
                            }
                        }

                        bytes = await downloadService.DownloadAsync(url);
                    }

                    await storageService.SaveFile(cacheRoot, GetCacheFileName(playItem.Url), bytes);
                }
            }
        }

        private static string[] urls = new string[] { "https://api.hag27.com/GETPUBMEDIALINKS",
                                                      "https://apps.jw.org/GETPUBMEDIALINKS"};

        private async Task<string> getBibleChapterUrl(string languageCode, string publicationCode, int bookNumber, int chapter)
        {
            try
            {
                var harvestLink1 = $@"{urls[0]}?booknum=0&output=json&pub={publicationCode}
&fileformat=MP3&langwritten={languageCode}&txtCMSLang=E&booknum={bookNumber}&track={chapter}";

                var harvestLink2 = $@"{urls[1]}?booknum=0&output=json&pub={publicationCode}
&fileformat=MP3&langwritten={languageCode}&txtCMSLang=E&booknum={bookNumber}&track={chapter}";

                var @bytes = await downloadService.DownloadAsync(harvestLink1, harvestLink2);
                string jsonString = Encoding.Default.GetString(@bytes);
                dynamic model = JsonConvert.DeserializeObject<dynamic>(jsonString);

                return model.files[languageCode].MP3[0].file.url;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> getMusicTrackUrl(string languageCode, string publicationCode, int trackNumber)
        {
            try
            {
                var harvestLink1 = $"{urls[0]}?booknum=0&output=json&pub={publicationCode}&fileformat=MP3" +
                                    $"{(languageCode == null ? $"&langwritten=E" : $"&langwritten={languageCode}")}" +
                                    $"&txtCMSLang=E&track={trackNumber}";

                var harvestLink2 = $@"{urls[1]}?booknum=0&output=json&pub={publicationCode}&fileformat=MP3" +
                                    $"{(languageCode == null ? $"&langwritten=E" : $"&langwritten={languageCode}")}" +
                                    $"&langwritten={languageCode}&txtCMSLang=E&track={trackNumber}";

                var @bytes = await downloadService.DownloadAsync(harvestLink1, harvestLink2);
                string jsonString = Encoding.Default.GetString(@bytes);
                dynamic model = JsonConvert.DeserializeObject<dynamic>(jsonString);

                return model.files[languageCode].MP3[0].file.url;
            }
            catch
            {
                return null;
            }

        }

        public async Task CleanUp()
        {
            var schedules = await scheduleDbContext.AlarmSchedules.ToListAsync();
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
