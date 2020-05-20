using Bible.Alarm.Contracts.Platform;
using Bible.Alarm.Services.Contracts;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Services
{
    public class MediaIndexService : IDisposable
    {
        private readonly Lazy<string> indexRoot;

        private IStorageService storageService;
        private IVersionFinder versionFinder;

        public string IndexRoot => indexRoot.Value;

        public MediaIndexService(IStorageService storageService, IVersionFinder versionFinder)
        {
            this.storageService = storageService;
            this.versionFinder = versionFinder;

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

            var mediaIndexDbExists = await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db"))
                            //verify that any previous unzipping process was not incomplete
                            && !await storageService.FileExists(tmpIndexFilePath);

            var versionFileExists = false;
            var isOutdatedVersion = false;

            //delete the file if it was outdated by an app auto-update.
            if (mediaIndexDbExists)
            {
                var versionFilePath = Path.Combine(IndexRoot, "version.dat");
                versionFileExists = await storageService.FileExists(versionFilePath);

                if (versionFileExists)
                {
                    var version = await storageService.ReadFile(versionFilePath);
                    var currentVersion = versionFinder.GetVersionName();

                    if (version != currentVersion)
                    {
                        isOutdatedVersion = true;
                    }
                }
            }

            return !mediaIndexDbExists || !versionFileExists || isOutdatedVersion;
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

            if (await storageService.FileExists(Path.Combine(IndexRoot, "mediaIndex.db")))
            {
                await storageService.DeleteFile(Path.Combine(IndexRoot, "mediaIndex.db"));
            }

            if (CurrentDevice.RuntimePlatform == Device.Android && 
                await storageService.FileExists(Path.Combine(IndexRoot, defaultAlarmFile)))
            {
                await storageService.DeleteFile(Path.Combine(IndexRoot, defaultAlarmFile));
            }

            await storageService.CopyResourceFile(indexResourceFile, IndexRoot, indexResourceFile);
            ZipFile.ExtractToDirectory(tmpIndexFilePath, IndexRoot);
            
            if(CurrentDevice.RuntimePlatform == Device.Android)
            {
                await storageService.CopyResourceFile(defaultAlarmFile, IndexRoot, defaultAlarmFile);
            }

            await storageService.DeleteFile(tmpIndexFilePath);
            await storageService.SaveFile(IndexRoot, "version.dat", versionFinder.GetVersionName());
        }

        public void Dispose()
        {
            storageService.Dispose();
            @lock.Dispose();
        }
    }
}
