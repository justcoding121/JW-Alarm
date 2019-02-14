﻿using Advanced.Algorithms.DataStructures.Foundation;
using JW.Alarm.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class MediaService
    {
        private MediaIndexService mediaLookUpService;
        private MediaDbContext dbContext;

        public MediaService(MediaIndexService mediaLookUpService, MediaDbContext dbContext)
        {
            this.mediaLookUpService = mediaLookUpService;
            this.dbContext = dbContext;
        }

        public async Task<Dictionary<string, Language>> GetBibleLanguages()
        {
            await mediaLookUpService.Verify();
            return await dbContext.BibleTranslations.Select(x => x.Language).Distinct().ToDictionaryAsync(x => x.Code, x => x);
        }

        public async Task<Dictionary<string, BibleTranslation>> GetBibleTranslations(string languageCode)
        {
            await mediaLookUpService.Verify();
            return await dbContext.BibleTranslations.Where(x => x.Language.Code == languageCode).ToDictionaryAsync(x => x.Code, x => x);
        }

        public async Task<OrderedDictionary<int, BibleBook>> GetBibleBooks(string languageCode, string versionCode)
        {
            await mediaLookUpService.Verify();

            var books = await dbContext.BibleTranslations.Where(x => x.Language.Code == languageCode)
                                                         .Where(x => x.Code == versionCode)
                                                         .Select(x => x.Books)
                                                         .FirstAsync();

            return new OrderedDictionary<int, BibleBook>(books.Select(x => new KeyValuePair<int, BibleBook>(x.Number, x)));
        }

        public async Task<OrderedDictionary<int, BibleChapter>> GetBibleChapters(string languageCode, string versionCode, int bookNumber)
        {
            await mediaLookUpService.Verify();

            var chapters = await dbContext.BibleTranslations
                .Where(x => x.Language.Code == languageCode)
                .Where(x => x.Code == versionCode)
                .SelectMany(x => x.Books)
                .Where(x => x.Number == bookNumber)
                .SelectMany(x => x.Chapters)
                .Include(x => x.Source)
                .ToListAsync();

            return new OrderedDictionary<int, BibleChapter>(chapters.Select(x => new KeyValuePair<int, BibleChapter>(x.Number, x)));
        }

        public async Task<Dictionary<string, MelodyMusic>> GetMelodyMusicReleases()
        {
            await mediaLookUpService.Verify();

            return await dbContext.MelodyMusic.ToDictionaryAsync(x => x.Code, x => x);
        }

        public async Task<OrderedDictionary<int, MusicTrack>> GetMelodyMusicTracks(string publicationCode)
        {
            await mediaLookUpService.Verify();

            var tracks = await dbContext.MelodyMusic
                .Where(x => x.Code == publicationCode)
                .SelectMany(x => x.Tracks)
                .Include(x => x.Source)
                .ToListAsync();

            return new OrderedDictionary<int, MusicTrack>(tracks.Select(x => new KeyValuePair<int, MusicTrack>(x.Number, x)));
        }

        public async Task<Dictionary<string, Language>> GetVocalMusicLanguages()
        {
            await mediaLookUpService.Verify();

            var languages = await dbContext.VocalMusic.Select(x => x.Language).Distinct().ToListAsync();
            return languages.ToDictionary(x => x.Code, x => x);
        }

        public async Task<Dictionary<string, VocalMusic>> GetVocalMusicReleases(string languageCode)
        {
            await mediaLookUpService.Verify();

            return await dbContext.VocalMusic.Where(x => x.Language.Code == languageCode).ToDictionaryAsync(x => x.Code, x => x);
        }

        public async Task<OrderedDictionary<int, MusicTrack>> GetVocalMusicTracks(string languageCode, string publicationCode)
        {
            await mediaLookUpService.Verify();

            var tracks = await dbContext.VocalMusic
                .Where(x => x.Language.Code == languageCode)
                .Where(x => x.Code == publicationCode)
                .SelectMany(x => x.Tracks)
                .Include(x => x.Source)
                .ToListAsync();

            return new OrderedDictionary<int, MusicTrack>(tracks.Select(x => new KeyValuePair<int, MusicTrack>(x.Number, x)));
        }

    }
}
