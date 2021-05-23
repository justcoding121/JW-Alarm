using System;
using System.IO;
using System.Xml;

namespace Bible.Alarm.VersionPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootDir = DirectoryHelper.IndexDirectory;

            patchAndroid(rootDir);
        }

        private static void patchAndroid(string rootDir)
        {
            var manifestFile = Path.Combine(rootDir, "src", "Bible.Alarm", "Bible.Alarm.Droid", "Properties", "AndroidManifest.xml");
            var doc = new XmlDocument();
            doc.Load(manifestFile);

            var sdkNode = doc.SelectSingleNode("/manifest");

            var attrs = sdkNode.Attributes;

            var versionCode = attrs["android:versionCode"].Value;
            var versionName = attrs["android:versionName"].Value;

            var newVersionCode = $"{int.Parse(versionCode) + 1}";

            var versionNameSplit = versionName.Split(".");
            var oldVersionMinor = int.Parse(versionNameSplit[1]);
            var oldVersionMajor = int.Parse(versionNameSplit[0]);
            
            var newVersionMinor = (oldVersionMinor % 99) == 0 ? 0 : oldVersionMinor + 1;
            var newVersionMajor = newVersionMinor == 0 ? oldVersionMajor + 1 : oldVersionMajor;
          
            var newVersionName = $"{newVersionMajor}.{newVersionMinor}";

            attrs["android:versionCode"].Value = newVersionCode;
            attrs["android:versionName"].Value = newVersionName;
            doc.Save(manifestFile);
        }
    }
}
