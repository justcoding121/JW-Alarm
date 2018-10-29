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
        private IAlarmScheduleService scheduleService;
        private IBibleReadingScheduleService bibleReadingScheduleService;
        private MediaService mediaService;

        public MediaPlayService(IAlarmScheduleService scheduleService,
            IBibleReadingScheduleService bibleReadingScheduleService,
            MediaService mediaService)
        {
            this.scheduleService = scheduleService;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
            this.mediaService = mediaService;
        }

        public abstract Task Play(int scheduleId);

        public async Task SetNextItemToPlay(int scheduleId, PlayType currentPublication)
        {
            var alarm = await scheduleService.Read(scheduleId);

            switch (currentPublication)
            {
                case PlayType.Bible:
                    await setNextBibleChapter(alarm);
                    break;
            }
        }

        private async Task setNextBibleChapter(AlarmSchedule schedule)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(schedule.BibleReadingScheduleId) as BibleReadingSchedule;
            var chapters = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var nextChapter = chapters.NextHigher(bibleReadingSchedule.ChapterNumber);

            if (!nextChapter.Equals(default(KeyValuePair<int, BibleChapter>)))
            {
                bibleReadingSchedule.ChapterNumber = nextChapter.Key;
                await bibleReadingScheduleService.Update(bibleReadingSchedule);
                return;
            }

            var books = await mediaService.GetBibleBooks(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode);
            var nextBook = books.NextHigher(bibleReadingSchedule.BookNumber);

            if(!nextBook.Equals(default(KeyValuePair<int, BibleBook>)))
            {
                bibleReadingSchedule.BookNumber = nextBook.Key;
                await bibleReadingScheduleService.Update(bibleReadingSchedule);
                return;
            }

            nextBook = books.Min();
            bibleReadingSchedule.BookNumber = nextBook.Key;
            await bibleReadingScheduleService.Update(bibleReadingSchedule);
            return;
        }

        public async Task<PlayItem> NextUrlToPlay(int scheduleId, PlayType playType)
        {
            var schedule = await scheduleService.Read(scheduleId);

            if (playType == PlayType.Music && schedule.MusicEnabled)
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
                    var melodyMusic = schedule.Music;
                    var melodyTracks = await mediaService.GetMelodyMusicTracks(melodyMusic.PublicationCode);
                    var melodyTrack = melodyTracks[melodyMusic.TrackNumber];
                    return new PlayItem(PlayType.Music, melodyTrack.Url);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[vocalMusic.TrackNumber];
                    return new PlayItem(PlayType.Music, vocalTrack.Url);
            }

            return null;
        }

        private async Task<PlayItem> nextBibleUrlToPlay(AlarmSchedule schedule)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(schedule.BibleReadingScheduleId) as BibleReadingSchedule;
            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var bibleTrack = bibleTracks[bibleReadingSchedule.ChapterNumber];
            return new PlayItem(PlayType.Bible, bibleTrack.Url);
        }

        public async virtual Task Stop(AlarmSchedule schedule)
        {
            await scheduleService.Update(schedule);
        }
    }
}
