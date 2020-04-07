using AudioLinkHarvester.Audio;
using AudioLinkHarvester.Bible;
using AudioLinkHarvester.Models;
using AudioLinkHarvestor.Utility;
using Bible.Alarm.Audio.Links.Harvestor;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AudioLinkHarvester
{
    public class Program
    {

        private static Dictionary<string, string> jwBiblePublicationCodeToNameMappings =
            new Dictionary<string, string>(new KeyValuePair<string, string>[]{
                new KeyValuePair<string, string>("nwt","New World Translation (2013)"),
                new KeyValuePair<string, string>("bi12","New World Translation (1984)"),
        });


        private static Dictionary<string, string> bgBiblePublicationCodeToNameMappings =
            new Dictionary<string, string>(new KeyValuePair<string, string>[]{
                new KeyValuePair<string, string>("kjv","King James Version"),
                new KeyValuePair<string, string>("nivuk","New International Version")
        });

        private static Dictionary<string, string> biblePublicationCodeToNameMappings =
            jwBiblePublicationCodeToNameMappings.Select(x => x)
                .Concat(bgBiblePublicationCodeToNameMappings).Select(x => x)
                    .ToDictionary(x => x.Key, x => x.Value);


        /// <summary>
        /// Harvest URL links to get the mp3 files liks for Bible & Music 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            deleteDirectory(DirectoryHelper.IndexDirectory);

            var bibleTasks = new List<Task>();

            var languageCodeToNameMappings = new ConcurrentDictionary<string, string>();
            var languageCodeToEditionsMapping = new ConcurrentDictionary<string, List<string>>();

            //////Bible
            bibleTasks.Add(JwBibleHarvester.Harvest_Bible_Links(jwBiblePublicationCodeToNameMappings, languageCodeToNameMappings, languageCodeToEditionsMapping));
            bibleTasks.Add(BgBibleHarvester.Harvest_Bible_Links(bgBiblePublicationCodeToNameMappings, languageCodeToNameMappings, languageCodeToEditionsMapping));

            var musicTasks = new List<Task>();

            ////////Music
            musicTasks.Add(MusicHarverster.Harvest_Vocal_Music_Links());
            musicTasks.Add(MusicHarverster.Harvest_Music_Melody_Links());

            Task.WhenAll(bibleTasks.Concat(musicTasks).ToArray()).Wait();

            writeBibleIndex(languageCodeToNameMappings, languageCodeToEditionsMapping);

            var index = new
            {
                ReleaseDate = DateTime.Now.Ticks
            };

            var indexFile = $"{DirectoryHelper.IndexDirectory}/media/index.json";
            if (File.Exists(indexFile))
            {
                File.Delete(indexFile);
            }

            File.WriteAllText(indexFile, JsonConvert.SerializeObject(index));

            DbSeeder.Seed($"{DirectoryHelper.IndexDirectory}").Wait();

            zipFiles();
        }

        private static void writeBibleIndex(ConcurrentDictionary<string, string> languageCodeToNameMappings, 
                ConcurrentDictionary<string, List<string>> languageCodeToEditionsMapping)
        {
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


        private static void zipFiles()
        {
            var zipIndex = $"{DirectoryHelper.IndexDirectory}/index.zip";
            if (File.Exists(zipIndex))
            {
                File.Delete(zipIndex);
            }

            ZipFile.CreateFromDirectory($"{Path.Combine(DirectoryHelper.IndexDirectory, "db")}", zipIndex);
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        private static void deleteDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                deleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
    }
}
