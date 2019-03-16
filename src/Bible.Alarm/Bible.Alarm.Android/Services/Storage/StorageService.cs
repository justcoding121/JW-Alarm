using Android.App;
using Android.Content.Res;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid
{
    public class DroidStorageService : IStorageService
    {
        public string StorageRoot => Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

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

        public async Task CopyResourceFile(string resourceFilePath,
            string destinationDirectoryPath, string destinationFileName)
        {
            AssetManager assets = Application.Context.Assets;
            using (StreamReader sr = new StreamReader(assets.Open(resourceFilePath)))
            {
                char[] buffer = new char[1024];
                using (BinaryWriter fileWriter =
                    new BinaryWriter(File.Create(Path.Combine(destinationDirectoryPath, destinationFileName))))
                {
                    long readCount = 0;
                    while (readCount < sr.BaseStream.Length)
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

        public async Task<DateTimeOffset> GetFileCreationDate(string path, bool isResourceFile)
        {
            throw new NotImplementedException();
        }
    }
}
