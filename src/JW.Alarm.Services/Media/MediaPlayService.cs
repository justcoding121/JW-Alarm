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
            var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber, bibleReadingSchedule.ChapterNumber);
            bibleReadingSchedule.BookNumber = next.Key.Number;
            bibleReadingSchedule.ChapterNumber = next.Value.Number;
            await bibleReadingScheduleService.Update(bibleReadingSchedule);
        }

        private async Task<KeyValuePair<BibleBook, BibleChapter>> getNextBibleChapter(string languageCode, string publicationCode, int bookNumber, int chapter)
        {
            var result = new KeyValuePair<BibleBook, BibleChapter>(new BibleBook(), new BibleChapter());

            var books = await mediaService.GetBibleBooks(languageCode, publicationCode);
            var currentBook = books[bookNumber];

            var chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, bookNumber);
            var nextChapter = chapters.NextHigher(chapter);

            if (!nextChapter.Equals(default(KeyValuePair<int, BibleChapter>)))
            {
                return new KeyValuePair<BibleBook, BibleChapter>(currentBook, nextChapter.Value);
            }

            var nextBook = books.NextHigher(bookNumber);


            if (!nextBook.Equals(default(KeyValuePair<int, BibleBook>)))
            {
                chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, nextBook.Key);
                return new KeyValuePair<BibleBook, BibleChapter>(nextBook.Value, chapters[0]);
            }

            nextBook = books.Min();
            chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, nextBook.Key);
            return new KeyValuePair<BibleBook, BibleChapter>(nextBook.Value, chapters[0]);
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
                    return new PlayItem(PlayType.Music, melodyTrack.Duration, melodyTrack.Url);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[vocalMusic.TrackNumber];
                    return new PlayItem(PlayType.Music, vocalTrack.Duration, vocalTrack.Url);
            }

            return null;
        }

        private async Task<PlayItem> nextBibleUrlToPlay(AlarmSchedule schedule)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(schedule.BibleReadingScheduleId) as BibleReadingSchedule;
            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var bibleTrack = bibleTracks[bibleReadingSchedule.ChapterNumber];
            return new PlayItem(PlayType.Bible, bibleTrack.Duration, bibleTrack.Url);
        }

        public async Task<List<PlayItem>> ItemsToPlay(int scheduleId, TimeSpan duration)
        {
            var result = new List<PlayItem>();

            var schedule = await scheduleService.Read(scheduleId);

            if (schedule.MusicEnabled)
            {
                result.Add(await nextMusicUrlToPlay(schedule));
            }

            if (schedule.BibleReadingEnabled)
            {
                var bibleReadingSchedule = await bibleReadingScheduleService.Read(schedule.BibleReadingScheduleId);

                int bookNumber = bibleReadingSchedule.BookNumber;
                int chapter = bibleReadingSchedule.ChapterNumber;
                var chapters = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bookNumber);

                var chapterDetail = chapters[chapter];

                var url = chapterDetail.Url;
                var trackDuration = chapterDetail.Duration;

                while (duration.TotalSeconds > 0)
                {
                    result.Add(new PlayItem(PlayType.Bible, trackDuration, url));
                    duration = duration.Subtract(trackDuration);

                    var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode,
                        bibleReadingSchedule.PublicationCode,
                        bookNumber, chapter);

                    bookNumber = next.Key.Number;
                    chapter = next.Value.Number;
                    trackDuration = next.Value.Duration;
                    url = next.Value.Url;
                }

            }

            return result;
        }
    }
}
