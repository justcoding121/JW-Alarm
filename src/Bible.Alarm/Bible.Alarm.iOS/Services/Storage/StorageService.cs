using System;
using System.IO;
using Bible.Alarm.Services;

namespace Bible.Alarm.Droid.Services.Storage
{
    public class iOSStorageService : StorageService
    {
        private string storageRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
        public override string StorageRoot
        {
            get
            {
                return storageRoot;
            }
        }
    }
}