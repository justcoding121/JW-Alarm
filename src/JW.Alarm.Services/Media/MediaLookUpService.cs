using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class MediaLookUpService
    {
        private readonly string latestIndexStatusFileUrl = "https://cdn.rawgit.com/justcoding121/JW-Alarm/master/src/server/index.json";
        private readonly string latestIndexFileUrl = "https://cdn.rawgit.com/justcoding121/JW-Alarm/master/src/server/index.zip";

        private readonly Lazy<string> indexRoot;

        private DownloadService downloadService;
        private IStorageService storageService;

        public string IndexRoot => indexRoot.Value;

        public MediaLookUpService(DownloadService downloadService, IStorageService storageService)
        {
            this.downloadService = downloadService;
            this.storageService = storageService;

            indexRoot = new Lazy<string>(() => Path.Combine(this.storageService.StorageRoot, "Media"));
        }

        private readonly SemaphoreSlim @lock = new SemaphoreSlim(1);

        public async Task Verify()
        {
            try
            {
                await @lock.WaitAsync();
                if (!await indexExists())
                {
                    await copyIndexFromResource();
                }
            }
            finally
            {
                @lock.Release();
            }
        }

        private Task<bool> indexExists()
        {
            return storageService.DirectoryExists(IndexRoot);
        }

        private async Task copyIndexFromResource()
        {
            var indexResourceFile = "Assets/Media/index.zip";
            await storageService.CopyResourceFile(indexResourceFile, IndexRoot, "index.zip");

            var indexFilePath = Path.Combine(IndexRoot, "index.zip");
            ZipFile.ExtractToDirectory(indexFilePath, IndexRoot);
            await storageService.DeleteFile(indexFilePath);
        }


        public async Task UpdateIndex()
        {
            await Verify();

            if (!await isIndexUpToDate())
            {
                var bytes = await downloadService.DownloadAsync(latestIndexFileUrl);
                await storageService.SaveFile(IndexRoot, "index.zip", bytes);

                var indexFilePath = Path.Combine(IndexRoot, "index.zip");
                ZipFile.ExtractToDirectory(indexFilePath, storageService.StorageRoot);
                await storageService.DeleteFile(indexFilePath);
            }
        }

        private async Task<bool> isIndexUpToDate()
        {
            await Verify();

            var currentIndexFileContents = await storageService.ReadFile(Path.Combine(storageService.StorageRoot, "index.json"));
            var currentIndex = JsonConvert.DeserializeObject<KeyValuePair<string, long>>(currentIndexFileContents);

            var latestIndexFileBytes = await downloadService.DownloadAsync(latestIndexFileUrl);
            var latestIndexFileContents = Encoding.UTF8.GetString(latestIndexFileBytes);
            var latestIndex = JsonConvert.DeserializeObject<KeyValuePair<string, long>>(latestIndexFileContents);

            return latestIndex.Value > currentIndex.Value;
        }
    }
}
