using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    public class PlaylistService : IPlaylistService
    {
        private ScheduleDbContext scheduleDbContext;
        private MediaService mediaService;

        public PlaylistService(ScheduleDbContext scheduleDbContext,
            MediaService mediaService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.mediaService = mediaService;
        }

        public async Task<PlayItem> NextTrack(long scheduleId)
        {
            var schedule = await scheduleDbContext.AlarmSchedules
                .AsNoTracking()
                .Include(x => x.Music)
                .Include(x => x.BibleReadingSchedule)
                .FirstAsync(x => x.Id == scheduleId);

            if (schedule.MusicEnabled)
            {
                return await nextMusicUrlToPlay(schedule);
            }
            else
            {
                return await nextBibleUrlToPlay(scheduleId, schedule.BibleReadingSchedule);
            }
        }

        public async Task<PlayItem> NextTrack(NotificationDetail currentTrack)
        {
            var schedule = await scheduleDbContext.AlarmSchedules
                                .AsNoTracking()
                                .Include(x => x.Music)
                                .Include(x => x.BibleReadingSchedule)
                                .FirstAsync(x => x.Id == currentTrack.ScheduleId);

            var bibleReadingSchedule = schedule.BibleReadingSchedule;

            if (currentTrack.PlayType == PlayType.Music)
            {
                return await nextBibleUrlToPlay(currentTrack.ScheduleId, bibleReadingSchedule);
            }

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
                BookNumber = bookNumber,
                PublicationCode = currentTrack.PublicationCode,
                LanguageCode = currentTrack.LanguageCode,
                ChapterNumber = chapter,
                Duration = bibleTrack.Source.Duration,
                LookUpPath = bibleTrack.Source.LookUpPath

            }, bibleTrack.Source.Duration, bibleTrack.Source.Url);
        }

        public async Task MarkTrackAsPlayed(NotificationDetail trackDetail)
        {
            var schedule = await scheduleDbContext.AlarmSchedules
                                    .Include(x => x.Music)
                                    .Include(x => x.BibleReadingSchedule)
                                    .FirstAsync(x => x.Id == trackDetail.ScheduleId);

            if (trackDetail.PlayType == PlayType.Music)
            {

                if (!schedule.Music.Repeat)
                {
                    schedule.Music.TrackNumber = trackDetail.TrackNumber;
                    var next = await nextMusicUrlToPlay(schedule, true);
                    schedule.Music.TrackNumber = next.PlayDetail.TrackNumber;
                }
            }
            else
            {
                var bibleReadingSchedule = schedule.BibleReadingSchedule;

                bibleReadingSchedule.BookNumber = trackDetail.BookNumber;
                bibleReadingSchedule.ChapterNumber = trackDetail.ChapterNumber;
                bibleReadingSchedule.FinishedDuration = trackDetail.FinishedDuration;
            }

            await scheduleDbContext.SaveChangesAsync();
        }

        public async Task MarkTrackAsFinished(NotificationDetail trackDetail)
        {
            var schedule = await scheduleDbContext.AlarmSchedules
                                    .Include(x => x.Music)
                                    .Include(x => x.BibleReadingSchedule)
                                    .FirstAsync(x => x.Id == trackDetail.ScheduleId);

            if (trackDetail.PlayType == PlayType.Music)
            {
                if (!schedule.Music.Repeat)
                {
                    var next = await nextMusicUrlToPlay(schedule, true);
                    schedule.Music.TrackNumber = next.PlayDetail.TrackNumber;
                }
            }
            else
            {
                var bibleReadingSchedule = schedule.BibleReadingSchedule;

                var next = await getNextBibleChapter(trackDetail.LanguageCode, trackDetail.PublicationCode, trackDetail.BookNumber, trackDetail.ChapterNumber);

                bibleReadingSchedule.BookNumber = next.Key.Number;
                bibleReadingSchedule.ChapterNumber = next.Value.Number;
                bibleReadingSchedule.FinishedDuration = default(TimeSpan);
            }

            await scheduleDbContext.SaveChangesAsync();
        }

        public async Task<List<PlayItem>> NextTracks(long scheduleId)
        {
            var result = new List<PlayItem>();

            var schedule = await scheduleDbContext.AlarmSchedules
                                    .AsNoTracking()
                                    .Include(x => x.Music)
                                    .Include(x => x.BibleReadingSchedule)
                                    .FirstAsync(x => x.Id == scheduleId);

            var numberOfChaptersToRead = schedule.NumberOfChaptersToRead;

            if (schedule.MusicEnabled)
            {
                result.Add(await nextMusicUrlToPlay(schedule));
            }

            var bibleReadingSchedule = schedule.BibleReadingSchedule;

            int bookNumber = bibleReadingSchedule.BookNumber;
            int chapter = bibleReadingSchedule.ChapterNumber;
            var chapters = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bookNumber);

            var chapterDetail = chapters[chapter];

            var publicationCode = bibleReadingSchedule.PublicationCode;
            var languageCode = bibleReadingSchedule.LanguageCode;
            var url = chapterDetail.Source.Url;
            var trackDuration = chapterDetail.Source.Duration;
            var lookUpPath = chapterDetail.Source.LookUpPath;

            bool markedSeekTrack = false;

            while (numberOfChaptersToRead > 0)
            {
                var notificationDetail = new NotificationDetail()
                {
                    ScheduleId = scheduleId,
                    PublicationCode = publicationCode,
                    LanguageCode = languageCode,
                    LookUpPath = lookUpPath,
                    BookNumber = bookNumber,
                    ChapterNumber = chapter,
                    Duration = trackDuration

                };

                //resume from where it was stopped last time
                if (!markedSeekTrack
                    && !schedule.AlwaysPlayFromStart
                    && !bibleReadingSchedule.FinishedDuration.Equals(default(TimeSpan))
                    && bibleReadingSchedule.LanguageCode == notificationDetail.LanguageCode
                    && bibleReadingSchedule.PublicationCode == notificationDetail.PublicationCode
                    && bookNumber == notificationDetail.BookNumber)
                {
                    notificationDetail.FinishedDuration = bibleReadingSchedule.FinishedDuration; 
                }

                markedSeekTrack = true;

                result.Add(new PlayItem(notificationDetail, trackDuration, url));

                numberOfChaptersToRead--;

                var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode,
                    bibleReadingSchedule.PublicationCode,
                    bookNumber, chapter);

                bookNumber = next.Key.Number;
                chapter = next.Value.Number;
                trackDuration = next.Value.Source.Duration;
                url = next.Value.Source.Url;
                lookUpPath = next.Value.Source.LookUpPath;
            }

            return result;
        }

        private async Task<KeyValuePair<BibleBook, BibleChapter>> getNextBibleChapter(string languageCode, string publicationCode, int bookNumber, int chapter)
        {
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
                return new KeyValuePair<BibleBook, BibleChapter>(nextBook.Value, chapters[1]);
            }

            nextBook = books.Min();
            chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, nextBook.Key);
            return new KeyValuePair<BibleBook, BibleChapter>(nextBook.Value, chapters[1]);
        }

        private async Task<PlayItem> nextMusicUrlToPlay(AlarmSchedule schedule, bool next = false)
        {
            switch (schedule.Music.MusicType)
            {
                case MusicType.Melodies:
                    var melodyMusic = schedule.Music;
                    var melodyTracks = await mediaService.GetMelodyMusicTracks(melodyMusic.PublicationCode);

                    var melodyTrack = melodyTracks[next ? (melodyMusic.TrackNumber % melodyTracks.Count) + 1 : melodyMusic.TrackNumber];
                    return new PlayItem(new NotificationDetail()
                    {
                        ScheduleId = schedule.Id,
                        PublicationCode = melodyMusic.PublicationCode,
                        TrackNumber = melodyTrack.Number,
                        Duration = melodyTrack.Source.Duration,
                        LookUpPath = melodyTrack.Source.LookUpPath
                    }, melodyTrack.Source.Duration, melodyTrack.Source.Url);

                case MusicType.Vocals:
                    var vocalMusic = schedule.Music;
                    var vocalTracks = await mediaService.GetVocalMusicTracks(vocalMusic.LanguageCode, vocalMusic.PublicationCode);
                    var vocalTrack = vocalTracks[next ? (vocalMusic.TrackNumber % vocalTracks.Count) + 1 : vocalMusic.TrackNumber];
                    return new PlayItem(new NotificationDetail()
                    {
                        ScheduleId = schedule.Id,
                        PublicationCode = vocalMusic.PublicationCode,
                        LanguageCode = vocalMusic.LanguageCode,
                        TrackNumber = vocalTrack.Number,
                        Duration = vocalTrack.Source.Duration,
                        LookUpPath = vocalTrack.Source.LookUpPath
                    }, vocalTrack.Source.Duration, vocalTrack.Source.Url);

                default:
                    throw new ApplicationException("Invalid MusicType.");
            }
        }

        private async Task<PlayItem> nextBibleUrlToPlay(long scheduleId, BibleReadingSchedule bibleReadingSchedule)
        {
            var bibleTracks = await mediaService.GetBibleChapters(bibleReadingSchedule.LanguageCode, bibleReadingSchedule.PublicationCode, bibleReadingSchedule.BookNumber);
            var bibleTrack = bibleTracks[bibleReadingSchedule.ChapterNumber];
            return new PlayItem(new NotificationDetail()
            {
                ScheduleId = scheduleId,
                PublicationCode = bibleReadingSchedule.PublicationCode,
                LanguageCode = bibleReadingSchedule.LanguageCode,
                BookNumber = bibleReadingSchedule.BookNumber,
                ChapterNumber = bibleReadingSchedule.ChapterNumber,
                Duration = bibleTrack.Source.Duration,
                LookUpPath = bibleTrack.Source.LookUpPath

            }, bibleTrack.Source.Duration, bibleTrack.Source.Url);
        }
    }
}
