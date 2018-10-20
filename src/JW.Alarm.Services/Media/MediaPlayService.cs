using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public abstract class MediaPlayService : IMediaPlayService
    {
        private IScheduleService scheduleService;
        private MediaService mediaService;

        public MediaPlayService(IScheduleService scheduleService, MediaService mediaService)
        {
            this.scheduleService = scheduleService;
            this.mediaService = mediaService;
        }

        public abstract Task Play(int scheduleId);

        public async Task SetNextItemToPlay(int scheduleId)
        {
            var alarm = (await scheduleService.Schedules)[scheduleId];

            switch (alarm.CurrentPlayItem)
            {
                case PlayType.Music:
                    await setNextBibleChapter(alarm, true);
                    break;
                case PlayType.Bible:
                    await setNextBibleChapter(alarm);
                    break;
            }

        }

        private async Task setNextBibleChapter(AlarmSchedule schedule, bool switchingPublication = false)
        {
            if (switchingPublication)
            {
                schedule.CurrentPlayItem = PlayType.Bible;
                await scheduleService.Update(schedule);
                return;
            }

            var bible = schedule.Bible as BibleAudio;
            var chapters = await mediaService.GetBibleChapters(bible.LanguageCode, bible.VersionCode, bible.BookNumber);
            if (bible.ChapterNumber < chapters.Count)
            {
                bible.ChapterNumber++;
                await scheduleService.Update(schedule);
                return;
            }

            var books = await mediaService.GetBibleBooks(bible.LanguageCode, bible.VersionCode);

            var nextBook = books.NextHigher(bible.BookNumber);

            bible.BookNumber = nextBook.Key;
            await scheduleService.Update(schedule);
            return;


        }

        public async Task UpdatePlayedSeconds(AlarmSchedule schedule, int second)
        {
            switch (schedule.CurrentPlayItem)
            {
                case PlayType.Music:
                    break;
                case PlayType.Bible:
                    (schedule.Bible as BibleAudio).Second = second;
                    await scheduleService.Update(schedule);
                    break;
            }
        }

        public async Task<PlayItem> NextUrlToPlay(int scheduleId)
        {
            var schedule = (await scheduleService.Schedules)[scheduleId];

            if (schedule.CurrentPlayItem == PlayType.Music)
            {
                return await nextMusicUrlToPlay(schedule);
            }
            else
            {
                return await nextBibleUrlToPlay(schedule);
            }
        }

        private async Task<PlayItem> nextMusicUrlToPlay(AlarmSchedule schedule)
        {
            switch (schedule.Music.MusicType)
            {
                case MusicType.Melodies:
                    var melodyMusic = schedule.Music as MelodyMusic;
                    var melodyTracks = await mediaService.GetMelodyMusicTracks(melodyMusic.VersionCode, melodyMusic.DiscCode);
                    var melodyTrack = melodyTracks[melodyMusic.TrackNumber];
                    return new PlayItem(melodyTrack.Url, 0);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music as VocalMusic;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.VersionCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[vocalMusic.TrackNumber];
                    return new PlayItem(vocalTrack.Url, 0);
            }

            return null;
        }

        private async Task<PlayItem> nextBibleUrlToPlay(AlarmSchedule schedule)
        {
            var bibleAudio = schedule.Bible;
            var bibleTracks = await mediaService.GetBibleChapters(bibleAudio.LanguageCode, bibleAudio.VersionCode, bibleAudio.BookNumber);
            var bibleTrack = bibleTracks[bibleAudio.ChapterNumber];
            return new PlayItem(bibleTrack.Url, bibleAudio.Second);
        }

        public async virtual Task Stop(AlarmSchedule schedule)
        {
            await scheduleService.Update(schedule);
        }
    }
}
