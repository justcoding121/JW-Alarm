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
        private IScheduleDbContext scheduleService;
        private IBibleReadingDbContext bibleReadingScheduleService;
        private MediaService mediaService;

        public PlaylistService(IScheduleDbContext scheduleService,
            IBibleReadingDbContext bibleReadingScheduleService,
            MediaService mediaService)
        {
            this.scheduleService = scheduleService;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
            this.mediaService = mediaService;
        }

        public async Task<PlayItem> NextTrack(long scheduleId)
        {
            var schedule = await scheduleService.Read(scheduleId);

            if (schedule.MusicEnabled)
            {
                return await nextMusicUrlToPlay(schedule);
            }
            else
            {
                return await nextBibleUrlToPlay(scheduleId, schedule.BibleReadingScheduleId);
            }
        }

        public async Task<PlayItem> NextTrack(NotificationDetail currentTrack)
        {
            if (currentTrack.PlayType == PlayType.Music)
            {
                return await nextBibleUrlToPlay(currentTrack.ScheduleId, currentTrack.BibleReadingScheduleId);
            }

            var bibleReadingSchedule = await bibleReadingScheduleService.Read(currentTrack.BibleReadingScheduleId);

            int bookNumber = currentTrack.BookNumber;
            int chapter = currentTrack.ChapterNumber;

            var bibleChapter = await getNextBibleChapter(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bookNumber, chapter);
            bookNumber = bibleChapter.Key.Number;
            chapter = bibleChapter.Value.Number;

            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bookNumber);
            var bibleTrack = bibleTracks[chapter];

            return new PlayItem(new NotificationDetail()
            {
                ScheduleId = currentTrack.ScheduleId,
                BibleReadingScheduleId = currentTrack.BibleReadingScheduleId,
                BookNumber = bookNumber,
                ChapterNumber = chapter,
                Duration = bibleTrack.Duration

            }, bibleTrack.Duration, bibleTrack.Url);
        }

        public async Task SetFinishedTrack(NotificationDetail trackDetail)
        {
            if (trackDetail.PlayType == PlayType.Music)
            {
                var schedule = await scheduleService.Read(trackDetail.ScheduleId);

                if (!schedule.Music.Fixed)
                {
                    var next = await nextMusicUrlToPlay(schedule, true);
                    schedule.Music.TrackNumber = next.PlayDetail.TrackNumber;
                    await scheduleService.Update(schedule);
                }
            }
            else
            {
                var bibleReadingSchedule = await bibleReadingScheduleService.Read(trackDetail.BibleReadingScheduleId);

                bibleReadingSchedule.BookNumber = trackDetail.BookNumber;
                bibleReadingSchedule.ChapterNumber = trackDetail.ChapterNumber;
                await bibleReadingScheduleService.Update(bibleReadingSchedule);
            }
        }

        public async Task<List<PlayItem>> Playlist(long scheduleId, TimeSpan duration)
        {
            var result = new List<PlayItem>();

            var schedule = await scheduleService.Read(scheduleId);

            if (schedule.MusicEnabled)
            {
                result.Add(await nextMusicUrlToPlay(schedule));
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
                result.Add(new PlayItem(new NotificationDetail()
                {
                    ScheduleId = scheduleId,
                    BibleReadingScheduleId = schedule.BibleReadingScheduleId,
                    BookNumber = bookNumber,
                    ChapterNumber = chapter,
                    Duration = trackDuration

                }, trackDuration, url));

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

        private async Task<PlayItem> nextMusicUrlToPlay(AlarmSchedule schedule, bool next = false)
        {
            switch (schedule.Music.MusicType)
            {
                case MusicType.Melodies:
                    var melodyMusic = schedule.Music;
                    var melodyTracks = await mediaService.GetMelodyMusicTracks(melodyMusic.PublicationCode);

                    var melodyTrack = melodyTracks[next ? (melodyMusic.TrackNumber + 1) % melodyTracks.Count : melodyMusic.TrackNumber];
                    return new PlayItem(new NotificationDetail()
                    {
                        ScheduleId = schedule.Id,
                        BibleReadingScheduleId = schedule.BibleReadingScheduleId,
                        TrackNumber = melodyTrack.Number,
                        Duration = melodyTrack.Duration,   
                    }, melodyTrack.Duration, melodyTrack.Url);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[next ? (vocalMusic.TrackNumber + 1) % vocalTracks.Count : vocalMusic.TrackNumber];
                    return new PlayItem(new NotificationDetail()
                    {
                        ScheduleId = schedule.Id,
                        BibleReadingScheduleId = schedule.BibleReadingScheduleId,
                        TrackNumber = vocalTrack.Number,
                        Duration = vocalTrack.Duration
                    }, vocalTrack.Duration, vocalTrack.Url);

                default:
                    throw new ApplicationException("Invalid MusicType.");
            }
        }

        private async Task<PlayItem> nextBibleUrlToPlay(long scheduleId, long bibleReadingScheduleId)
        {
            var bibleReadingSchedule = await bibleReadingScheduleService.Read(bibleReadingScheduleId) as BibleReadingSchedule;
            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var bibleTrack = bibleTracks[bibleReadingSchedule.ChapterNumber];
            return new PlayItem(new NotificationDetail()
            {
                ScheduleId = scheduleId,
                BibleReadingScheduleId = bibleReadingScheduleId,
                BookNumber = bibleReadingSchedule.BookNumber,
                ChapterNumber = bibleReadingSchedule.ChapterNumber,
                Duration = bibleTrack.Duration

            }, bibleTrack.Duration, bibleTrack.Url);
        }
    }
}
