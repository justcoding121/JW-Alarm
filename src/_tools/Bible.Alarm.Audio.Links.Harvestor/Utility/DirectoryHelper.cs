using System.IO;

namespace AudioLinkHarvestor.Utility
{
    public static class DirectoryHelper
    {
        internal static string IndexDirectory = "../../../../_index";
        public static void Ensure(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
