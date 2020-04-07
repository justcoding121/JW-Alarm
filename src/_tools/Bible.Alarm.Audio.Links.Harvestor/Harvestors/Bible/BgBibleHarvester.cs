using AudioLinkHarvester.Models;
using AudioLinkHarvester.Models.Bible;
using AudioLinkHarvester.Utility;
using AudioLinkHarvestor.Utility;
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
        private static string[] booksList = new string[] { "Gen", "Exod", "Lev", "Num", "Deut", "Josh", "Judg", "Ruth", "1Sam", "2Sam", "1Kgs", "2Kgs", "1Chr", "2Chr", "Ezra", "Neh", "Esth", "Job", "Ps", "Prov", "Eccl", "Song", "Isa", "Jer", "Lam", "Ezek", "Dan", "Hos", "Joel", "Amos", "Obad", "Jonah", "Mic", "Nah", "Hab", "Zeph", "Hag", "Zech", "Mal", "Matt", "Mark", "Luke", "John", "Acts", "Rom", "1Cor", "2Cor", "Gal", "Eph", "Phil", "Col", "1Thess", "2Thess", "1Tim", "2Tim", "Titus", "Phlm", "Heb", "Jas", "1Pet", "2Pet", "1John", "2John", "3John", "Jude", "Rev" };
        private static Dictionary<int, string> booksKeyMap = booksList
                                                       .Select((s, i) => new { i = i + 1, s })
                                                       .ToDictionary(x => x.i, x => x.s);

        private static Dictionary<string, string> authorsKeyMap = new Dictionary<string, string>(new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("kjv", "mims"),
            new KeyValuePair<string, string>("nivuk", "suchet")
        });

        private static Dictionary<int, string> bookNames = getBookNames();

        private static Dictionary<string, int> chapterNumbers = getChapterNumbers();

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
                              var harvestLink = $"{UrlHelper.BgIndexServiceBaseUrl}?osis={bookKey}.{chapter}&version={publicationCode}&author={author}";

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


        private static Dictionary<string, int> getChapterNumbers()
        {
            var chapterDict = new Dictionary<string, int>();

            chapterDict["Gen"] = 50;
            chapterDict["Exod"] = 40;
            chapterDict["Lev"] = 27;
            chapterDict["Num"] = 36;
            chapterDict["Deut"] = 34;
            chapterDict["Josh"] = 24;
            chapterDict["Judg"] = 21;
            chapterDict["Ruth"] = 4;
            chapterDict["1Sam"] = 31;
            chapterDict["2Sam"] = 24;
            chapterDict["1Kgs"] = 22;
            chapterDict["2Kgs"] = 25;
            chapterDict["1Chr"] = 29;
            chapterDict["2Chr"] = 36;
            chapterDict["Ezra"] = 10;
            chapterDict["Neh"] = 13;
            chapterDict["Esth"] = 10;
            chapterDict["Job"] = 42;
            chapterDict["Ps"] = 150;
            chapterDict["Prov"] = 31;
            chapterDict["Eccl"] = 12;
            chapterDict["Song"] = 8;
            chapterDict["Isa"] = 66;
            chapterDict["Jer"] = 52;
            chapterDict["Lam"] = 5;
            chapterDict["Ezek"] = 48;
            chapterDict["Dan"] = 12;
            chapterDict["Hos"] = 14;
            chapterDict["Joel"] = 3;
            chapterDict["Amos"] = 9;
            chapterDict["Obad"] = 1;
            chapterDict["Jonah"] = 4;
            chapterDict["Mic"] = 7;
            chapterDict["Nah"] = 3;
            chapterDict["Hab"] = 3;
            chapterDict["Zeph"] = 3;
            chapterDict["Hag"] = 2;
            chapterDict["Zech"] = 14;
            chapterDict["Mal"] = 4;
            chapterDict["Matt"] = 28;
            chapterDict["Mark"] = 16;
            chapterDict["Luke"] = 24;
            chapterDict["John"] = 21;
            chapterDict["Acts"] = 28;
            chapterDict["Rom"] = 16;
            chapterDict["1Cor"] = 16;
            chapterDict["2Cor"] = 13;
            chapterDict["Gal"] = 6;
            chapterDict["Eph"] = 6;
            chapterDict["Phil"] = 4;
            chapterDict["Col"] = 4;
            chapterDict["1Thess"] = 5;
            chapterDict["2Thess"] = 3;
            chapterDict["1Tim"] = 6;
            chapterDict["2Tim"] = 4;
            chapterDict["Titus"] = 3;
            chapterDict["Phlm"] = 1;
            chapterDict["Heb"] = 13;
            chapterDict["Jas"] = 5;
            chapterDict["1Pet"] = 5;
            chapterDict["2Pet"] = 3;
            chapterDict["1John"] = 5;
            chapterDict["2John"] = 1;
            chapterDict["3John"] = 1;
            chapterDict["Jude"] = 1;
            chapterDict["Rev"] = 22;

            return chapterDict;
        }

        private static Dictionary<int, string> getBookNames()
        {
            return new string[] { "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy", "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel", "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra", "Nehemiah", "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah", "Haggai", "Zechariah", "Malachi", "Matthew", "Mark", "Luke", "John", "Acts (of the Apostles)", "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians", "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy", "2 Timothy", "Titus", "Philemon", "Hebrews", "James", "1 Peter", "2 Peter", "1 John", "2 John", "3 John", "Jude", "Revelation" }
                       .Select((s, i) => new { i = i + 1, s })
                       .ToDictionary(x => x.i, x => x.s);
        }
    }
}
