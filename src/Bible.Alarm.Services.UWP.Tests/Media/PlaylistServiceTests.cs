using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP.Tests.Media
{
    [TestClass]
    public class PlaylistServiceTests
    {
        [TestMethod]
        public async Task Playlist_Service_Smoke_Test()
        {
            var storageService = new UwpStorageService();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);
            await indexService.Verify();

            var mediaService = new MediaService(indexService, storageService);


            var db = new ScheduleDbContext();
            db.Database.EnsureCreated();

            var name = $"Test Alarm";
            var newRecord = new AlarmSchedule()
            {
                Name = name,
                DaysOfWeek =
                    DaysOfWeek.Sunday |
                    DaysOfWeek.Monday |
                    DaysOfWeek.Tuesday |
                    DaysOfWeek.Wednesday |
                    DaysOfWeek.Thursday |
                    DaysOfWeek.Friday |
                    DaysOfWeek.Saturday,
                Hour = 10,
                Minute = 30,
                IsEnabled = true,
                Music = new AlarmMusic()
                {
                    MusicType = MusicType.Melodies,
                    PublicationCode = "iam",
                    LanguageCode = "E",
                    TrackNumber = 89
                },

                BibleReadingSchedule = new BibleReadingSchedule()
                {
                    BookNumber = 23,
                    ChapterNumber = 1,
                    LanguageCode = "E",
                    PublicationCode = "nwt"
                }
            };

            db.AlarmSchedules.Add(newRecord);
            db.SaveChanges();

            var playlistService = new PlaylistService(db, mediaService);

            var track = await playlistService.NextTrack(newRecord.Id);
            var tracks = await playlistService.NextTracks(newRecord.Id, TimeSpan.FromHours(1));

            Assert.IsTrue(tracks.Count > 0);
            Assert.AreEqual(track.Url, tracks[0].Url);

            db.SaveChanges();

            track = await playlistService.NextTrack(newRecord.Id);
            tracks = await playlistService.NextTracks(newRecord.Id, TimeSpan.FromHours(1));

            Assert.IsTrue(tracks.Count > 0);
            Assert.AreEqual(track.Url, tracks[0].Url);

            Assert.IsNotNull(track);
            Assert.AreEqual(PlayType.Music, track.PlayDetail.PlayType);
            Assert.AreEqual(89, track.PlayDetail.TrackNumber);

            await playlistService.MarkTrackAsFinished(track.PlayDetail);

            track = await playlistService.NextTrack(track.PlayDetail);

            Assert.IsNotNull(track);
            Assert.AreEqual(PlayType.Bible, track.PlayDetail.PlayType);
            Assert.AreEqual(23, track.PlayDetail.BookNumber);
            Assert.AreEqual(1, track.PlayDetail.ChapterNumber);

            await playlistService.MarkTrackAsFinished(track.PlayDetail);

            track = await playlistService.NextTrack(track.PlayDetail);

            Assert.IsNotNull(track);
            Assert.AreEqual(PlayType.Bible, track.PlayDetail.PlayType);
            Assert.AreEqual(23, track.PlayDetail.BookNumber);
            Assert.AreEqual(2, track.PlayDetail.ChapterNumber);

            await playlistService.MarkTrackAsFinished(track.PlayDetail);

        }


        private class FakeDownloadService : IDownloadService
        {
            public Task<byte[]> DownloadAsync(string url, string alternativeUrl = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
