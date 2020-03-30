using Bible.Alarm.Services;
using System;
using System.IO;

namespace Bible.Alarm.Droid.Services.Storage
{
    public class iOSStorageService : StorageService
    {
        private static string storageRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
        public override string StorageRoot
        {
            get
            {
                return storageRoot;
            }
        }

        private static string cacheRoot = Path.GetTempPath();
        public override string CacheRoot => cacheRoot;
    }
}