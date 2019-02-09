using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudioLinkHarvestor.Utility
{
    public static class DirectoryHelper
    {
        internal static string IndexDirectory = "../../../../../docs";
        public static void Ensure(string directory)
        {
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
