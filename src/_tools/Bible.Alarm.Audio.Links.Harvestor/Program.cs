using AudioLinkHarvester.Audio;
using AudioLinkHarvester.Bible;
using AudioLinkHarvestor.Utility;
using Bible.Alarm.Audio.Links.Harvestor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace AudioLinkHarvester
{
    public class Program
    {

        /// <summary>
        /// Harvest URL links to get the mp3 files liks for Bible & Music 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            deleteDirectory(DirectoryHelper.IndexDirectory);

            var tasks = new List<Task>();

            //////Bible
            tasks.Add(BibleHarvester.Harvest_Bible_Links());

            //////Music
            tasks.Add(MusicHarverster.Harvest_Vocal_Music_Links());
            tasks.Add(MusicHarverster.Harvest_Music_Melody_Links());

            Task.WaitAll(tasks.ToArray());

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
