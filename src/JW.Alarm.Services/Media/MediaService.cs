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
        private MediaLookUpService mediaLookUpService;
        private IStorageService storageService;

        public MediaService(MediaLookUpService mediaLookUpService, IStorageService storageService)
        {
            this.mediaLookUpService = mediaLookUpService;
            this.storageService = storageService;
        }

        public async Task<Dictionary<string, string>> GetBibleLanguages()
        {
            var root = mediaLookUpService.IndexRoot;
            var languageIndex = Path.Combine(root, "Audio", "Bible", "index.json");
            var languages = await storageService.ReadFile(languageIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(languages);
        }

        public async Task<Dictionary<string, string>> GetBibleVersions(string languageCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var bibleIndex = Path.Combine(root, "Audio", "Bible", languageCode, "index.json");
            var bibleVersions = await storageService.ReadFile(bibleIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(bibleVersions);
        }

        public async Task<TreeDictionary<int, string>> GetBibleBooks(string languageCode, string versionCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var booksIndex = Path.Combine(root, "Audio", "Bible", languageCode, versionCode, "index.json");
            var bibleBooks = await storageService.ReadFile(booksIndex);
            return JsonConvert.DeserializeObject<TreeDictionary<int, string>>(bibleBooks);
        }

        public async Task<TreeDictionary<int, BibleChapter>> GetBibleChapters(string languageCode, string versionCode, int bookNumber)
        {
            var root = mediaLookUpService.IndexRoot;
            var booksIndex = Path.Combine(root, "Audio", "Bible", languageCode, versionCode, bookNumber.ToString(), "index.json");
            var bibleBooks = await storageService.ReadFile(booksIndex);
            return JsonConvert.DeserializeObject<BibleBook>(bibleBooks).Chapters;
        }

        public async Task<Dictionary<string, string>> GetMelodyMusicVersions()
        {
            var root = mediaLookUpService.IndexRoot;
            var discIndex = Path.Combine(root, "Music", "Melodies", "index.json");
            var fileContent = await storageService.ReadFile(discIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
        }

        public async Task<Dictionary<string, string>> GetMelodyMusicDiscs(string versionCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var discIndex = Path.Combine(root, "Music", "Melodies", versionCode, "index.json");
            var fileContent = await storageService.ReadFile(discIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
        }

        public async Task<TreeDictionary<int, MusicTrack>> GetMelodyMusicTracks(string versionCode, string discCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Melodies", discCode, "index.json");
            var fileContent = await storageService.ReadFile(trackIndex);
            return JsonConvert.DeserializeObject<MusicDisc>(fileContent).Tracks;
        }

        public async Task<Dictionary<string, string>> GetVocalMusicLanguages()
        {
            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Vocals", "index.json");
            var melodyDiscs = await storageService.ReadFile(trackIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(melodyDiscs);
        }


        public async Task<Dictionary<string, string>> GetVocalMusicDiscs(string languageCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var discIndex = Path.Combine(root, "Music", "Vocals", languageCode, "index.json");
            var vocalDiscs = await storageService.ReadFile(discIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(vocalDiscs);
        }

        public async Task<Dictionary<string, string>> GetVocalMusicVersions(string languageCode, string versionCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Vocals", languageCode, versionCode, "index.json");
            var melodyDiscs = await storageService.ReadFile(trackIndex);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(melodyDiscs);
        }

        public async Task<TreeDictionary<int, MusicTrack>> GetVocalMusicTracks(string languageCode, string versionCode, string discCode)
        {
            var root = mediaLookUpService.IndexRoot;
            var trackIndex = Path.Combine(root, "Music", "Vocals", languageCode, versionCode, discCode, "index.json");
            var melodyDiscs = await storageService.ReadFile(trackIndex);
            return JsonConvert.DeserializeObject<MusicDisc>(melodyDiscs).Tracks;
        }

    }
}
