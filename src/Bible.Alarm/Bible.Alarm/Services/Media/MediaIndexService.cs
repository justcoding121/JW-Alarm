using Bible.Alarm.Services.Contracts;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    public class MediaIndexService : IDisposable
    {
        private readonly Lazy<string> indexRoot;

        private IStorageService storageService;

        public string IndexRoot => indexRoot.Value;

        public MediaIndexService(IStorageService storageService)
        {
            this.storageService = storageService;

            indexRoot = new Lazy<string>(() => this.storageService.StorageRoot);
        }

        private readonly SemaphoreSlim @lock = new SemaphoreSlim(1);
        private static bool verified = false;

        public async Task Verify()
        {
            try
            {
                await @lock.WaitAsync();

                if (verified)
                {
                    return;
                }

                if (await indexDoNotExistOrIsOutdated())
                {
                    await clearCopyIndexFromResource();
                }

                verified = true;
            }
            finally
            {
                @lock.Release();
            }
        }

        private async Task<bool> indexDoNotExistOrIsOutdated()
        {
            var tmpIndexFilePath = Path.Combine(IndexRoot, "index.zip");

            var exists = await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db"))
                            //verify that any previous unzipping process was not incomplete
                            && !await storageService.FileExists(tmpIndexFilePath);

            var outdated = false;

            //delete the file if it was outdated by an app auto-update.
            if (exists)
            {
                var resourceFileCreationDate = await storageService.GetFileCreationDate("index.zip", true);
                var creationDate = await storageService.GetFileCreationDate(Path.Combine(IndexRoot, "mediaIndex.db"), false);

                if (resourceFileCreationDate > creationDate)
                {
                    outdated = true;
                }
            }

            return !exists || outdated;
        }

        private async Task clearCopyIndexFromResource()
        {
            var indexResourceFile = "index.zip";
            var defaultAlarmFile = "cool-alarm-tone-notification-sound.mp3";

            var tmpIndexFilePath = Path.Combine(IndexRoot, indexResourceFile);

            if (await storageService.FileExists(tmpIndexFilePath))
            {
                await storageService.DeleteFile(tmpIndexFilePath);
            }

            if(await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db")))
            {
                await storageService.DeleteFile(Path.Combine(IndexRoot, "mediaIndex.db"));
            }

            if (await storageService.FileExists(Path.Combine(IndexRoot, defaultAlarmFile)))
            {
                await storageService.DeleteFile(Path.Combine(IndexRoot, defaultAlarmFile));
            }

            await storageService.CopyResourceFile(indexResourceFile, IndexRoot, indexResourceFile);   
            ZipFile.ExtractToDirectory(tmpIndexFilePath, IndexRoot);
            await storageService.CopyResourceFile(defaultAlarmFile, IndexRoot, defaultAlarmFile);

            await storageService.DeleteFile(tmpIndexFilePath);
        }

        public void Dispose()
        {
            storageService.Dispose();
            @lock.Dispose();
        }
    }
}
