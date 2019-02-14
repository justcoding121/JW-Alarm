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
    public class MediaIndexService
    {
        //private readonly string latestIndexStatusFileUrl = "https://cdn.rawgit.com/justcoding121/JW-Media-Index/master/src/server/index.json";
        private readonly string latestIndexFileUrl = "https://cdn.rawgit.com/justcoding121/W-Media-Index/master/src/server/index.zip";

        private readonly Lazy<string> indexRoot;

        private IDownloadService downloadService;
        private IStorageService storageService;

        public string IndexRoot => indexRoot.Value;

        public MediaIndexService(IDownloadService downloadService, IStorageService storageService)
        {
            this.downloadService = downloadService;
            this.storageService = storageService;

            indexRoot = new Lazy<string>(() => Path.Combine(this.storageService.StorageRoot));
        }

        private readonly SemaphoreSlim @lock = new SemaphoreSlim(1);
        private bool verified = false;

        public async Task Verify()
        {
            try
            {
                await @lock.WaitAsync();
                if (verified)
                {
                    return;
                }
                if (!await indexExists())
                {
                    await copyIndexFromResource();
                    verified = true;
                }
            }
            finally
            {
                @lock.Release();
            }
        }

        private async Task<bool> indexExists()
        {
            var tmpIndexFilePath = Path.Combine(IndexRoot, "index.zip");

            return await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db"))
                //verify that any previous unzipping process was not incomplete
                && !await storageService.FileExists(tmpIndexFilePath);

        }

        private async Task copyIndexFromResource()
        {
            var indexResourceFile = "Assets/Media/index.zip";
            await storageService.CopyResourceFile(indexResourceFile, IndexRoot, "index.zip");
            var tmpIndexFilePath = Path.Combine(IndexRoot, "index.zip");
            ZipFile.ExtractToDirectory(tmpIndexFilePath, IndexRoot);
            await storageService.DeleteFile(tmpIndexFilePath);
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
