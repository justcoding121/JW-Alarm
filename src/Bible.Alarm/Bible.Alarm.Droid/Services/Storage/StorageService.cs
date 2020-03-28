using System;
using Bible.Alarm.Services;

namespace Bible.Alarm.Droid.Services.Storage
{
    public class AndroidStorageService : StorageService
    {
        public override string StorageRoot
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        public override string CacheRoot
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }
    }
}