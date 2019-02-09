using AudioLinkHarvester.Models;
using AudioLinkHarvester.Models.Bible;
using AudioLinkHarvester.Utility;
using AudioLinkHarvestor.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioLinkHarvester.Bible
{
    internal class BibleHarvester
    {
        private static Dictionary<string, string> biblePublicationCodeToNameMappings = new Dictionary<string, string>(new KeyValuePair<string, string>[]{
            new KeyValuePair<string, string>("nwt","New World Translation (2013)"),
            new KeyValuePair<string, string>("bi12","New World Translation (1984)")
        });


        internal async static Task Harvest_Bible_Links()
        {
            var tasks = new List<Task>();

            var languageCodeToEditionsMapping = new Dictionary<string, List<string>>();
            var languageCodeToNameMappings = new Dictionary<string, string>();

            foreach (var publication in biblePublicationCodeToNameMappings)
            {
                var publicationCode = publication.Key;

                var harvestLink = $"{UrlHelper.IndexServiceBaseUrl}?booknum=0&output=json&pub={publicationCode}&fileformat=MP3&alllangs=1&langwritten=E&txtCMSLang=E";

                var jsonString = await DownloadUtility.GetAsync(harvestLink);
                var model = JsonConvert.DeserializeObject<dynamic>(jsonString);

                foreach (var item in model.languages)
                {
                    var languageCode = item.Name;
                    var language = model.languages[item.Name]["name"].Value;

                    languageCodeToNameMappings[languageCode] = language;

                    Console.WriteLine($"Harvesting Bible chapter links for {publication.Value} of {language} language.");
                    await BibleHarvester.harvestBibleLinks(languageCode, publicationCode);

                    if (languageCodeToEditionsMapping.ContainsKey(languageCode))
                    {
                        languageCodeToEditionsMapping[languageCode].Add(publication.Key);
                    }
                    else
                    {
                        languageCodeToEditionsMapping[languageCode] = new List<string>(new[] { publication.Key });
                    }
                    break;
                }

                break;
            }

            if (!Directory.Exists($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible"))
            {
                Directory.CreateDirectory($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible");
            }

            File.WriteAllText($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/languages.json", JsonConvert.SerializeObject(
                languageCodeToEditionsMapping.Select(x =>
                new Language
                {
                    Code = x.Key,
                    Name = languageCodeToNameMappings[x.Key]
                }).OrderBy(x => x.Code).ToList()));

            foreach (var languageEditionsMap in languageCodeToEditionsMapping)
            {
                if (!Directory.Exists($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/{languageEditionsMap.Key}"))
                {
                    Directory.CreateDirectory($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/{languageEditionsMap.Key}");
                }

                File.WriteAllText($"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/{languageEditionsMap.Key}/publications.json", JsonConvert.SerializeObject(
                languageEditionsMap.Value.Select(x =>
                new Publication
                {
                    Code = x,
                    Name = biblePublicationCodeToNameMappings[x]
                }).OrderBy(x => x.Code)));
            }

        }

        private static async Task<bool> harvestBibleLinks(string languageCode, string publicationCode)
        {
            var booksDirectory = $"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/{languageCode}/{publicationCode}";
            var booksIndex = $"{booksDirectory}/books.json";

            var bookNumberBookMap = new Dictionary<int, BibleBook>();
            var bookNumberChapterMap = new Dictionary<int, Dictionary<int, BibleChapter>>();

            var bookNumber = 1;

            var harvestLink = $"{UrlHelper.IndexServiceBaseUrl}?output=json&pub={publicationCode}&fileformat=MP3&alllangs=0&langwritten={languageCode}&txtCMSLang=E";

            while (bookNumber <= 66)
            {
                var jsonString = await DownloadUtility.GetAsync(harvestLink);

                dynamic model = null;

                try
                {
                    model = JsonConvert.DeserializeObject<dynamic>(jsonString);
                }
                catch
                {
                    bookNumber++;
                    harvestLink = $"{UrlHelper.IndexServiceBaseUrl}?output=json&pub={publicationCode}&booknum={bookNumber}&fileformat=MP3&alllangs=0&langwritten={languageCode}&txtCMSLang=E";

                    continue;
                }

                var bookFiles = model.files[languageCode].MP3;
                foreach (var bookFile in bookFiles)
                {
                    string url = bookFile.file.url;

                    if (bookFile.track == 0
                    || url.EndsWith(".zip")) continue;

                    bookNumber = bookFile.booknum;

                    if (!bookNumberBookMap.ContainsKey(bookNumber))
                    {
                        var name = harvestLink.Contains("booknum=") ? model.pubName : bookFile.title.ToString().Split('-')[0].Trim();
                        name = name == "Psalm 1" ? "Psalms" : name;
                        bookNumberBookMap[bookNumber] = new BibleBook()
                        {
                            Number = bookFile.booknum,
                            Name = name
                        };
                    }

                    int trackNumber = bookFile.track;
                    double duration = bookFile.duration;
                    if (!bookNumberChapterMap.ContainsKey(bookNumber))
                    {
                        bookNumberChapterMap[bookNumber] = new Dictionary<int, BibleChapter>();
                    }

                    if (!bookNumberChapterMap[bookNumber].ContainsKey(trackNumber))
                    {
                        bookNumberChapterMap[bookNumber].Add(trackNumber,
                        new BibleChapter()
                        {
                            Number = trackNumber,
                            Url = bookFile.file.url,
                            Duration = TimeSpan.FromMilliseconds(duration * 1000)
                        });
                    }
                }

                if (harvestLink.Contains("booknum="))
                {
                    bookNumber++;
                }

                harvestLink = $"{UrlHelper.IndexServiceBaseUrl}?output=json&pub={publicationCode}&booknum={bookNumber}&fileformat=MP3&alllangs=0&langwritten={languageCode}&txtCMSLang=E";
            }

            if (bookNumberBookMap.Count > 0)
            {
                if (!Directory.Exists(booksDirectory))
                {
                    Directory.CreateDirectory(booksDirectory);
                }

                File.WriteAllText(booksIndex, JsonConvert.SerializeObject(bookNumberBookMap.Select(x =>
                new BibleBook()
                {
                    Number = x.Key,
                    Name = x.Value.Name
                }).OrderBy(x => x.Number)));

                foreach (var book in bookNumberBookMap)
                {
                    var directory = $"{booksDirectory}/{book.Value.Number}";
                    DirectoryHelper.Ensure(directory);

                    var chapterIndex = $"{directory}/chapters.json";
                    File.WriteAllText(chapterIndex, JsonConvert.SerializeObject(
                    bookNumberChapterMap[book.Key]
                    .Select(x => x.Value)
                    .OrderBy(x => x.Number)
                    .ToList()));
                }

                return true;

            }

            return false;
        }

    }
}
