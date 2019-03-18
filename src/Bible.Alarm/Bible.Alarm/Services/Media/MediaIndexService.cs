using JW.Alarm.Services.Contracts;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public class MediaIndexService
    {
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

            var exists = await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db"))
                //verify that any previous unzipping process was not incomplete
                && !await storageService.FileExists(tmpIndexFilePath);

            //delete the file if it was outdated by an app auto-update.
            if (exists)
            {
                var resourceFileCreationDate = await storageService.GetFileCreationDate("Assets/Media/index.zip", true);
                var creationDate = await storageService.GetFileCreationDate(Path.Combine(IndexRoot, "mediaIndex.db"), false);

                if(creationDate < resourceFileCreationDate)
                {
                    await storageService.DeleteFile(Path.Combine(IndexRoot, "mediaIndex.db"));
                    return false;
                }
            }

            return exists;
        }

        private async Task copyIndexFromResource()
        {
            var indexResourceFile = "Bible.Alarm.Resources.index.zip";
            await storageService.CopyResourceFile(indexResourceFile, IndexRoot, "index.zip");
            var tmpIndexFilePath = Path.Combine(IndexRoot, "index.zip");
            ZipFile.ExtractToDirectory(tmpIndexFilePath, IndexRoot);
            await storageService.DeleteFile(tmpIndexFilePath);
        }

    }
}
