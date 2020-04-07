using AudioLinkHarvester.Models;
using AudioLinkHarvester.Models.Bible;
using AudioLinkHarvester.Utility;
using AudioLinkHarvestor.Utility;
using Bible.Alarm.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioLinkHarvester.Bible
{
    internal class BgBibleHarvester
    {
        private static Dictionary<int, string> booksKeyMap = BgSourceHelper.BookNumberToBookCodeMap;

        private static Dictionary<string, string> authorsKeyMap = BgSourceHelper.PublisherCodeToAuthorsCodeMap;

        private static Dictionary<int, string> bookNames = BgSourceHelper.BookNumberToNamesMap;

        private static Dictionary<string, int> chapterNumbers = BgSourceHelper.BookCodeToTotalChaptersMap;

        internal async static
            Task Harvest_Bible_Links(Dictionary<string, string> biblePublicationCodeToNameMappings,
                                        ConcurrentDictionary<string, string> languageCodeToNameMappings,
                                        ConcurrentDictionary<string, List<string>> languageCodeToEditionsMapping)
        {
            foreach (var publication in biblePublicationCodeToNameMappings)
            {
                var languageCode = "E";
                var language = "English";

                languageCodeToNameMappings.TryAdd(languageCode, language);

                Console.WriteLine($"Harvesting Bible chapter links for {publication.Value} of {language} language.");
                await harvestBgBibleLinks(languageCode, publication.Key);

                if (!languageCodeToEditionsMapping
                        .TryAdd(languageCode, new List<string>(new[] { publication.Key })))
                {
                    languageCodeToEditionsMapping[languageCode].Add(publication.Key);
                }
            }
        }

        private static async Task<bool> harvestBgBibleLinks(string languageCode, string publicationCode)
        {
            var booksDirectory = $"{DirectoryHelper.IndexDirectory}/media/Audio/Bible/{languageCode}/{publicationCode}";
            var booksIndex = $"{booksDirectory}/books.json";

            var bookNumberBookMap = new ConcurrentDictionary<int, BibleBook>();
            var bookNumberChapterMap = new ConcurrentDictionary<int, ConcurrentDictionary<int, BibleChapter>>();

            var tasks = Enumerable.Range(1, 66)
            .Select(bookNumber => Task.Run(async () =>
                {
                    var bookKey = booksKeyMap[bookNumber];

                    var chapterTasks = Enumerable.Range(1, chapterNumbers[bookKey])
                          .Select(chapter => Task.Run(async () =>
                          {
                              var author = authorsKeyMap[publicationCode];
                              var harvestLink = $"{UrlHelper.BibleGatewayIndexServiceBaseUrl}?osis={bookKey}.{chapter}&version={publicationCode}&author={author}";

                              var jsonString = await DownloadUtility.GetAsync(harvestLink);
                              var model = JsonConvert.DeserializeObject<dynamic>(jsonString);

                              var hashKey = model["curHash"];

                              bookNumberBookMap.TryAdd(bookNumber, new BibleBook()
                              {
                                  Number = bookNumber,
                                  Name = bookNames[bookNumber]
                              });

                              int trackNumber = chapter;

                              bookNumberChapterMap.TryAdd(bookNumber, new ConcurrentDictionary<int, BibleChapter>());

                              bookNumberChapterMap[bookNumber].TryAdd(trackNumber,
                              new BibleChapter()
                              {
                                  Number = trackNumber,
                                  Url = $"https://stream.biblegateway.com/bibles/32/{publicationCode}-{author}/{bookKey}.{chapter}.{hashKey}.mp3",
                              });

                          })).ToList();

                    await Task.WhenAll(chapterTasks);

                })).ToList();

            await Task.WhenAll(tasks);

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
