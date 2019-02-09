using Advanced.Algorithms.DataStructures.Foundation;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class MediaService 
    {
        private MediaIndexService mediaLookUpService;
        private IStorageService storageService;

        public MediaService(MediaIndexService mediaLookUpService, IStorageService storageService)
        {
            this.mediaLookUpService = mediaLookUpService;
            this.storageService = storageService;
        }

        public async Task<Dictionary<string, Language>> GetBibleLanguages()
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var languageIndex = Path.Combine(root, "Audio", "Bible", "languages.json");
            var languages = await storageService.ReadFile(languageIndex);
            return JsonConvert.DeserializeObject<IEnumerable<Language>>(languages).ToDictionary(x=>x.Code, x=>x);
        }

        public async Task<Dictionary<string, Publication>> GetBibleTranslations(string languageCode)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var bibleIndex = Path.Combine(root, "Audio", "Bible", languageCode, "publications.json");
            var bibleTranslations = await storageService.ReadFile(bibleIndex);
            return JsonConvert.DeserializeObject<IEnumerable<Publication>>(bibleTranslations).ToDictionary(x => x.Code, x => x);
        }

        public async Task<OrderedDictionary<int, BibleBook>> GetBibleBooks(string languageCode, string versionCode)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var booksIndex = Path.Combine(root, "Audio", "Bible", languageCode, versionCode, "books.json");
            var bibleBooks = await storageService.ReadFile(booksIndex);
            return new OrderedDictionary<int, BibleBook>(JsonConvert.DeserializeObject<IEnumerable<BibleBook>>(bibleBooks)
                                                    .Select(x=>new KeyValuePair<int, BibleBook>(x.Number, x)));
        }

        public async Task<OrderedDictionary<int, BibleChapter>> GetBibleChapters(string languageCode, string versionCode, int bookNumber)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var booksIndex = Path.Combine(root, "Audio", "Bible", languageCode, versionCode, bookNumber.ToString(), "chapters.json");
            var bibleChapters = await storageService.ReadFile(booksIndex);
            return new OrderedDictionary<int, BibleChapter>(JsonConvert.DeserializeObject<IEnumerable<BibleChapter>>(bibleChapters)
                                                       .Select(x => new KeyValuePair<int, BibleChapter>(x.Number, x)));
        }

        public async Task<Dictionary<string, Publication>> GetMelodyMusicReleases()
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var releaseIndex = Path.Combine(root, "Music", "Melodies", "publications.json");
            var fileContent = await storageService.ReadFile(releaseIndex);
            return JsonConvert.DeserializeObject<IEnumerable<Publication>>(fileContent).ToDictionary(x=>x.Code, x=>x);
        }

        public async Task<OrderedDictionary<int, MusicTrack>> GetMelodyMusicTracks(string publicationCode)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Melodies", publicationCode, "tracks.json");
            var fileContent = await storageService.ReadFile(trackIndex);
            return new OrderedDictionary<int, MusicTrack>(JsonConvert.DeserializeObject<IEnumerable<MusicTrack>>(fileContent)
                                                    .Select(x => new KeyValuePair<int, MusicTrack>(x.Number, x)));
        }

        public async Task<Dictionary<string, Language>> GetVocalMusicLanguages()
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var languageIndex = Path.Combine(root, "Music", "Vocals", "languages.json");
            var languages = await storageService.ReadFile(languageIndex);
            return JsonConvert.DeserializeObject<IEnumerable<Language>>(languages).ToDictionary(x=>x.Code, x=>x);
        }

        public async Task<Dictionary<string, Publication>> GetVocalMusicReleases(string languageCode)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var releaseIndex = Path.Combine(root, "Music", "Vocals", languageCode, "publications.json");
            var vocalReleases = await storageService.ReadFile(releaseIndex);
            return JsonConvert.DeserializeObject<IEnumerable<Publication>>(vocalReleases).ToDictionary(x => x.Code, x => x);
        }

        public async Task<OrderedDictionary<int, MusicTrack>> GetVocalMusicTracks(string languageCode, string publicationCode)
        {
            await mediaLookUpService.Verify();

            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Vocals", languageCode, publicationCode, "tracks.json");
            var melodyTracks = await storageService.ReadFile(trackIndex);
            return new OrderedDictionary<int, MusicTrack>(JsonConvert.DeserializeObject<IEnumerable<MusicTrack >>(melodyTracks)
                                                    .Select(x=>new KeyValuePair<int, MusicTrack>(x.Number, x)));
        }

    }
}
