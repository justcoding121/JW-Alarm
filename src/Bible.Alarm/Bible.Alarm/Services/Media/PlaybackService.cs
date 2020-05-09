using Advanced.Algorithms.DataStructures.Foundation;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Contracts.Network;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Library;
using MediaManager.Media;
using MediaManager.Playback;
using MediaManager.Player;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Services
{
    public class PlaybackService : IPlaybackService, IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private readonly IMediaManager mediaManager;
        private IAlarmService alarmService;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IStorageService storageService;
        private INetworkStatusService networkStatusService;
        private INotificationService notificationService;
        private IDownloadService downloadService;
        private IToastService toastService;

        private static long currentScheduleId;
        private Dictionary<IMediaItem, NotificationDetail> currentlyPlaying
            = new Dictionary<IMediaItem, NotificationDetail>();

        private SemaphoreSlim @lock = new SemaphoreSlim(1);

        private IMediaItem firstChapter;

        private bool readyTodispose;
        long IPlaybackService.CurrentlyPlayingScheduleId => currentScheduleId;

        public event EventHandler<MediaPlayerState> Stopped;

        public PlaybackService(IMediaManager mediaManager,
            IPlaylistService playlistService,
            IAlarmService alarmService,
            IMediaCacheService cacheService,
            IStorageService storageService,
            INetworkStatusService networkStatusService,
            INotificationService notificationService,
            IDownloadService downloadService,
            IToastService toastService)
        {
            this.mediaManager = mediaManager;
            this.playlistService = playlistService;
            this.alarmService = alarmService;
            this.cacheService = cacheService;
            this.storageService = storageService;
            this.networkStatusService = networkStatusService;
            this.notificationService = notificationService;
            this.downloadService = downloadService;
            this.toastService = toastService;

            this.mediaManager.MediaItemFinished += markTrackAsFinished;
            this.mediaManager.StateChanged += stateChanged;
        }

        private Task watcher;
        private bool wasPlaying;
        private DateTime playStartTime = DateTime.Now.AddDays(7);

        private static IMediaExtractor mediaExtractor => CrossMediaManager.Current.Extractor;

        public async Task Play(long scheduleId)
        {
            //already playing
            if (this.mediaManager.IsPreparedEx())
            {
                Dispose();
                Stopped?.Invoke(this, MediaPlayerState.Stopped);
                return;
            }

            playStartTime = DateTime.Now;

            await @lock.WaitAsync();

            try
            {
                currentScheduleId = scheduleId;

                var nextTracks = await playlistService.NextTracks(scheduleId);

                var downloadedTracks = new OrderedDictionary<int, FileInfo>();
                var streamingTracks = new OrderedDictionary<int, string>();

                var playDetailMap = new Dictionary<int, NotificationDetail>();

                var internetOn = await networkStatusService.IsInternetAvailable();

                var i = 0;
                foreach (var item in nextTracks)
                {
                    playDetailMap[i] = item.PlayDetail;

                    if (await cacheService.Exists(item.Url))
                    {
                        downloadedTracks.Add(i, new FileInfo(this.cacheService.GetCacheFilePath(item.Url)));
                    }
                    else
                    {
                        if (internetOn)
                        {
                            streamingTracks.Add(i, item.Url);
                        }
                        else
                        {
                            break;
                        }
                    }
                    i++;
                }

                var preparedTracks = 0;
                var totalTracks = nextTracks.Count;

                await toastService.Clear();
                Messenger<object>.Publish(MvvmMessages.ShowMediaProgessModal);
                Messenger<object>.Publish(MvvmMessages.MediaProgress, new Tuple<int, int>(preparedTracks, totalTracks));

                var downloadedMediaItems = (await Task.WhenAll(downloadedTracks.Select(x =>
                {
                    return Task.Run(async () =>
                    {
                        IMediaItem item;

                        if (CurrentDevice.RuntimePlatform == Device.UWP)
                        {
                            item = new MediaItem(x.Value.FullName);
                            //TODO: Fix this
                        }
                        else
                        {
                            item = await mediaExtractor.CreateMediaItem(x.Value);
                        }

                        item.SetDisplay(playDetailMap[x.Key]);
                        Messenger<object>.Publish(MvvmMessages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                        return item;
                    });

                }))).ToList();

                var streamableMediaItems = (await Task.WhenAll(streamingTracks.Select(x =>
               {
                   return Task.Run(async () =>
                   {
                       var playDetail = playDetailMap[x.Key];

                       try
                       {
                           if (await downloadService.FileExists(x.Value))
                           {
                               var item = await mediaExtractor.CreateMediaItem(x.Value);
                               item.SetDisplay(playDetail);
                               Messenger<object>.Publish(MvvmMessages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                               return item;
                           }
                           else
                           if (playDetail.IsBibleReading)
                           {
                               var url = await cacheService.GetBibleChapterUrl(playDetail.LanguageCode,
                                                playDetail.PublicationCode, playDetail.BookNumber, playDetail.ChapterNumber,
                                                playDetail.LookUpPath);

                               if (await downloadService.FileExists(url))
                               {
                                   var item = await mediaExtractor.CreateMediaItem(url);
                                   item.SetDisplay(playDetail);
                                   Messenger<object>.Publish(MvvmMessages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                                   return item;
                               }
                           }
                           else
                           {
                               var url = await cacheService.GetMusicTrackUrl(playDetail.LanguageCode, playDetail.LookUpPath);

                               if (await downloadService.FileExists(url))
                               {
                                   var item = await mediaExtractor.CreateMediaItem(url);
                                   item.SetDisplay(playDetail);
                                   Messenger<object>.Publish(MvvmMessages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                                   return item;
                               }
                           }

                           logger.Error($"Could'nt download the streaming file: {x.Value}.");
                           return null;
                       }
                       catch (Exception e)
                       {
                           logger.Error(e, $"An error happened when streaming file: {x.Value}.");
                           return null;
                       }

                   });

               }))).ToList();

                var mergedMediaItems = new OrderedDictionary<int, IMediaItem>();

                i = 0;
                foreach (var item in downloadedTracks)
                {
                    mergedMediaItems.Add(item.Key, downloadedMediaItems[i]);
                    i++;
                }

                i = 0;
                foreach (var item in streamingTracks)
                {
                    if (streamableMediaItems[i] != null)
                    {
                        mergedMediaItems.Add(item.Key, streamableMediaItems[i]);
                    }

                    i++;
                }

                Messenger<object>.Publish(MvvmMessages.HideMediaProgressModal, null);

                //play default ring tone if we don't have the files downloaded
                //and internet is not available
                if (!downloadedMediaItems.Any() && !await networkStatusService.IsInternetAvailable())
                {
                    await playBeep();
                }
                else
                {
                    currentlyPlaying = new Dictionary<IMediaItem, NotificationDetail>();

                    i = 0;
                    foreach (var track in mergedMediaItems)
                    {
                        if (track.Key != i)
                        {
                            break;
                        }

                        currentlyPlaying.Add(track.Value, playDetailMap[track.Key]);
                        i++;
                    }

                    if (!currentlyPlaying.Any())
                    {
                        await playBeep();
                    }
                    else
                    {
                        firstChapter = currentlyPlaying.FirstOrDefault(x => x.Value.IsBibleReading).Key;
                        this.mediaManager.RepeatMode = RepeatMode.Off;
                        await this.mediaManager.Play(mergedMediaItems.Select(x => x.Value));
                    }

                    Messenger<object>.Publish(MvvmMessages.ShowAlarmModal);
                }
            }
            finally
            {
                @lock.Release();
            }

            if (watcher == null)
            {
                watcher = Task.Run(async () =>
                {
                    while (!disposed)
                    {
                        await @lock.WaitAsync();

                        try
                        {
                            if (this.mediaManager.IsPlaying())
                            {
                                wasPlaying = true;

                                var mediaItem = this.mediaManager.Queue.Current;
                                if (currentlyPlaying.ContainsKey(mediaItem))
                                {
                                    var track = currentlyPlaying[mediaItem];

                                    if (track.FinishedDuration.TotalSeconds > 0
                                        && firstChapter != null
                                        && mediaItem == firstChapter)
                                    {
                                        await this.mediaManager.SeekTo(track.FinishedDuration);
                                        firstChapter = null;
                                    }
                                    else if (mediaManager.Position.TotalSeconds > 0)
                                    {
                                        if (mediaItem == firstChapter)
                                        {
                                            firstChapter = null;
                                        }

                                        track.FinishedDuration = mediaManager.Position;
                                        await this.playlistService.MarkTrackAsPlayed(track);
                                    }
                                }
                            }

                            //was playing and now stopped or play stopped without even getting played for last 60 seconds
                            if ((wasPlaying || DateTime.Now.Subtract(playStartTime).TotalSeconds > 60)
                                && this.mediaManager.IsStopped())
                            {
                                readyTodispose = true;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "An error happened when updating finished track duration.");
                        }
                        finally
                        {
                            @lock.Release();
                        }

                        if (readyTodispose)
                        {
                            Messenger<object>.Publish(MvvmMessages.HideAlarmModal, null);
                            Dispose();
                            Stopped?.Invoke(this, MediaPlayerState.Stopped);

                            break;
                        }

                        await Task.Delay(1000);
                    }
                });
            }
        }

        private async Task playBeep()
        {
            if (CurrentDevice.RuntimePlatform == Device.Android)
            {
                this.mediaManager.RepeatMode = RepeatMode.All;
                await this.mediaManager.Play(new FileInfo(Path.Combine(this.storageService.StorageRoot, "cool-alarm-tone-notification-sound.mp3")));
            }
            else
            {
                await toastService.ShowMessage("An error happened while downloading.");
            }
        }

        public async Task Snooze()
        {
            await @lock.WaitAsync();

            try
            {
                if (this.mediaManager.IsPreparedEx() && !readyTodispose)
                {
                    await this.mediaManager.StopEx();
                    await this.alarmService.Snooze(currentScheduleId);
                    currentScheduleId = 0;
                }
            }
            finally
            {
                @lock.Release();
            }

            readyTodispose = true;
        }

        public async Task Dismiss()
        {
            if (this.mediaManager.IsPreparedEx()
                 && !readyTodispose)
            {
                await this.mediaManager.StopEx();
            }

            currentScheduleId = 0;
            readyTodispose = true;
        }



        private async void stateChanged(object sender, StateChangedEventArgs e)
        {
            var mediaItem = this.mediaManager.Queue.Current;
            if (currentlyPlaying.ContainsKey(mediaItem))
            {
                var track = currentlyPlaying[mediaItem];

                switch (e.State)
                {
                    case MediaPlayerState.Playing:
                        if (track.FinishedDuration.TotalSeconds > 0
                            && firstChapter != null
                            && mediaItem == firstChapter)
                        {
                            await this.mediaManager.SeekTo(track.FinishedDuration);
                            firstChapter = null;
                        }
                        break;
                }
            }

        }

        private async void markTrackAsFinished(object sender, MediaItemEventArgs e)
        {
            await @lock.WaitAsync();

            try
            {
                if (currentlyPlaying.ContainsKey(e.MediaItem))
                {
                    var track = currentlyPlaying[e.MediaItem];

                    if (track.IsLastTrack)
                    {
                        await playlistService.MarkTrackAsFinished(track);
                        await Dismiss();
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when marking track as finished.");
            }
            finally
            {
                @lock.Release();
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                this.mediaManager.MediaItemFinished -= markTrackAsFinished;

                this.playlistService.Dispose();
                this.alarmService.Dispose();
                this.cacheService.Dispose();
                this.storageService.Dispose();
                this.networkStatusService.Dispose();
                this.notificationService.Dispose();

                @lock.Dispose();
            }
        }
    }
}
