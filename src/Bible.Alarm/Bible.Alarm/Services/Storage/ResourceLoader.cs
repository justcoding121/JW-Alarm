﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bible.Alarm.Services
{
    /// <summary>
    /// Utility class that can be used to find and load embedded resources into memory.
    /// </summary>
    /// <remarks>
    /// https://github.com/xamarin/mobile-samples/blob/master/EmbeddedResources/SharedLib/ResourceLoader.cs
    /// </remarks>
    public static class ResourceLoader
    {
        /// <summary>
        /// Attempts to find and return the given resource from within the specified assembly.
        /// </summary>
        /// <returns>The embedded resource stream.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="resourceFileName">Resource file name.</param>
        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
        {
            var resourceNames = assembly.GetManifestResourceNames();

            var resourcePaths = resourceNames
                .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();

            if (!resourcePaths.Any())
            {
                throw new Exception(string.Format("Resource ending with {0} not found.", resourceFileName));
            }

            if (resourcePaths.Count() > 1)
            {
                throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resourceFileName, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));
            }

            return assembly.GetManifestResourceStream(resourcePaths.Single());
        }

        public static FileInfo GetFileInfo(Assembly assembly)
        {
            return new FileInfo(assembly.Location);
        }
    }
}