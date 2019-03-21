using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Services
{
    public class StorageService : IStorageService
    {
        public string StorageRoot
        {
            get
            {
                return Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            }
        }

        public string ResourceRoot
        {
            get
            {
                var appRoot = AppDomain.CurrentDomain.BaseDirectory;

                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        return Path.Combine(appRoot, "Resources");

                    case Device.Android:
                        return Path.Combine(appRoot, "Assets");
                }

                return Path.Combine(appRoot, "Assets", "Media");
            }
        }



        public Task DeleteFile(string path)
        {
            File.Delete(path);
            return Task.FromResult(false);
        }

        public Task<bool> FileExists(string path)
        {
            return Task.FromResult(File.Exists(path));
        }

        public Task<bool> DirectoryExists(string path)
        {
            return Task.FromResult(Directory.Exists(path));
        }

        public Task DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
            return Task.CompletedTask;
        }

        public async Task<List<string>> GetAllFiles(string path)
        {
            if (!await DirectoryExists(path))
            {
                return new List<string>();
            }

            return Directory.GetFiles(path).ToList();
        }

        public Task<string> ReadFile(string path)
        {
            return Task.FromResult(File.ReadAllText(path));
        }

        public async Task SaveFile(string directoryPath, string name, string contents)
        {
            if (!await DirectoryExists(directoryPath))
            {
                await createDirectory(directoryPath);
            }

            File.WriteAllText(Path.Combine(directoryPath, name), contents);
        }

        public async Task SaveFile(string directoryPath, string name, byte[] contents)
        {
            if (!await DirectoryExists(directoryPath))
            {
                await createDirectory(directoryPath);
            }

            File.WriteAllBytes(Path.Combine(directoryPath, name), contents);
        }

        public async Task CopyResourceFile(string resourceFileName,
            string destinationDirectoryPath, string destinationFileName)
        {
            using (var sr = File.OpenRead(Path.Combine(ResourceRoot, resourceFileName)))
            {
                var buffer = new byte[1024];
                using (BinaryWriter fileWriter =
                    new BinaryWriter(File.Create(Path.Combine(destinationDirectoryPath, destinationFileName))))
                {
                    long readCount = 0;
                    while (readCount < sr.Length)
                    {
                        int read = await sr.ReadAsync(buffer, 0, buffer.Length);
                        readCount += read;
                        fileWriter.Write(buffer, 0, read);
                    }

                }
            }

        }

        private Task createDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetFileCreationDate(string pathOrName, bool isResourceFile)
        {
            FileInfo file = null;

            if (isResourceFile)
            {
                file = new FileInfo(Path.Combine(ResourceRoot, pathOrName));
            }
            else
            {
                file = new FileInfo(pathOrName);
            }

            return Task.FromResult(new DateTimeOffset(file.CreationTime));
        }
    }
}
