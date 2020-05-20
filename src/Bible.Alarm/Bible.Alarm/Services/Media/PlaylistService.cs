using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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

        public async Task MarkTrackAsPlayed(NotificationDetail trackDetail)
        {
            var trackChanged = false;

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

                if (bibleReadingSchedule.BookNumber != trackDetail.BookNumber
                      || bibleReadingSchedule.ChapterNumber != trackDetail.ChapterNumber)
                {
                    trackChanged = true;
                }

                bibleReadingSchedule.BookNumber = trackDetail.BookNumber;
                bibleReadingSchedule.ChapterNumber = trackDetail.ChapterNumber;
                bibleReadingSchedule.FinishedDuration = trackDetail.FinishedDuration;
            }

            await scheduleDbContext.SaveChangesAsync();

            if (trackChanged)
            {
                Messenger<object>.Publish(MvvmMessages.TrackChanged, schedule.Id);
            }
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

            Messenger<object>.Publish(MvvmMessages.TrackChanged, schedule.Id);
        }

        public async Task<List<PlayItem>> NextTracks(long scheduleId)
        {
            var result = new List<PlayItem>();

            var schedule = await scheduleDbContext.AlarmSchedules
                                    .AsNoTracking()
                                    .Include(x => x.Music)
                                    .Include(x => x.BibleReadingSchedule)
                                    .FirstOrDefaultAsync(x => x.Id == scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException($"Invalid schedule Id {scheduleId}");
            }

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
                    IsLastTrack = numberOfChaptersToRead == 1 ? true : false
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

                result.Add(new PlayItem(notificationDetail, url));

                numberOfChaptersToRead--;

                var next = await getNextBibleChapter(bibleReadingSchedule.LanguageCode,
                    bibleReadingSchedule.PublicationCode,
                    bookNumber, chapter);

                bookNumber = next.Key.Number;
                chapter = next.Value.Number;
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
                        LookUpPath = melodyTrack.Source.LookUpPath
                    }, melodyTrack.Source.Url);

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
                        LookUpPath = vocalTrack.Source.LookUpPath
                    }, vocalTrack.Source.Url);

                default:
                    throw new ApplicationException("Invalid MusicType.");
            }
        }

        public void Dispose()
        {
            scheduleDbContext.Dispose();
            mediaService.Dispose();
        }
    }
}
