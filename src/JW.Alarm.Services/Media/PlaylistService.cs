using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class PlaylistService : IPlaylistService
    {
        private IAlarmScheduleService scheduleService;
        private IBibleReadingScheduleService bibleReadingScheduleService;
        private MediaService mediaService;

        public PlaylistService(IAlarmScheduleService scheduleService,
            IBibleReadingScheduleService bibleReadingScheduleService,
            MediaService mediaService)
        {
            this.scheduleService = scheduleService;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
            this.mediaService = mediaService;
        }

        public async Task<PlayItem> NextTrack(int scheduleId)
        {
            var schedule = await scheduleService.Read(scheduleId);

            if (schedule.MusicEnabled)
            {
                return await nextMusicUrlToPlay(schedule);
            }
            else
            {
                return await nextBibleUrlToPlay(scheduleId);
            }
        }

        public async Task SetFinishedTrack(PlayDetail trackDetail)
        {
            await setNextBibleChapter(trackDetail.ScheduleId);
        }

        public async Task<List<string>> Playlist(int scheduleId, TimeSpan duration)
        {
            var result = new List<string>();

            var schedule = await scheduleService.Read(scheduleId);

            if (schedule.MusicEnabled)
            {
                result.Add((await nextMusicUrlToPlay(schedule)).Url);
            }

            var bibleReadingSchedule = await bibleReadingScheduleService.Read(schedule.BibleReadingScheduleId);

            int bookNumber = bibleReadingSchedule.BookNumber;
            int chapter = bibleReadingSchedule.ChapterNumber;
            var chapters = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bookNumber);

            var chapterDetail = chapters[chapter];

            var url = chapterDetail.Url;
            var trackDuration = chapterDetail.Duration;

            while (duration.TotalSeconds > 0)
            {
                result.Add(url);

                duration = duration.Subtract(trackDuration);

                var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode,
                    bibleReadingSchedule.PublicationCode,
                    bookNumber, chapter);

                bookNumber = next.Key.Number;
                chapter = next.Value.Number;
                trackDuration = next.Value.Duration;
                url = next.Value.Url;
            }

            return result;
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

        private async Task setNextBibleChapter(int scheduleId)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(scheduleId) as BibleReadingSchedule;
            var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber, bibleReadingSchedule.ChapterNumber);
            bibleReadingSchedule.BookNumber = next.Key.Number;
            bibleReadingSchedule.ChapterNumber = next.Value.Number;
            await bibleReadingScheduleService.Update(bibleReadingSchedule);
        }

        private async Task<PlayItem> nextMusicUrlToPlay(AlarmSchedule schedule)
        {
            switch (schedule.Music.MusicType)
            {
                case MusicType.Melodies:
                    var melodyMusic = schedule.Music;
                    var melodyTracks = await mediaService.GetMelodyMusicTracks(melodyMusic.PublicationCode);
                    var melodyTrack = melodyTracks[melodyMusic.TrackNumber];
                    return new PlayItem(new PlayDetail()
                    {
                        ScheduleId = schedule.Id,
                        TrackNumber = melodyTrack.Number
                    }, melodyTrack.Duration, melodyTrack.Url);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[vocalMusic.TrackNumber];
                    return new PlayItem(new PlayDetail()
                    {
                        ScheduleId = schedule.Id,
                        TrackNumber = vocalTrack.Number
                    }, vocalTrack.Duration, vocalTrack.Url);

                default:
                    throw new ApplicationException("Invalid MusicType.");
            }
        }

        private async Task<PlayItem> nextBibleUrlToPlay(int scheduleId)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(scheduleId) as BibleReadingSchedule;
            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var bibleTrack = bibleTracks[bibleReadingSchedule.ChapterNumber];
            return new PlayItem(new PlayDetail()
            {
                ScheduleId = scheduleId,
                BookNumber = bibleReadingSchedule.BookNumber,
                Chapter = bibleReadingSchedule.ChapterNumber

            }, bibleTrack.Duration, bibleTrack.Url);
        }
    }
}
