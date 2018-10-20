using JW.Alarm.Services.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace JW.Alarm.Services.Uwp
{
    public class UwpStorageService : IStorageService
    {
        public string StorageRoot => ApplicationData.Current.LocalFolder.Path;

        public async Task DeleteFile(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            await file.DeleteAsync();
        }
        public async Task<bool> FileExists(string path)
        {
            var directoryPath = path.Substring(0, path.LastIndexOf("\\"));
            var fileName = path.Substring(path.LastIndexOf("\\") + 1);

            if (!await DirectoryExists(directoryPath))
            {
                return false;
            }

            var directory = await StorageFolder.GetFolderFromPathAsync(directoryPath);

            return await directory.TryGetItemAsync(fileName) != null; ;
        }

        public async Task<bool> DirectoryExists(string path)
        {
            var subPath = path.Replace(StorageRoot, string.Empty);
            var subDirectories = subPath.Split('\\');

            var current = StorageRoot;
            var currentDirectory = await StorageFolder.GetFolderFromPathAsync(StorageRoot);

            foreach (var directory in subDirectories.Where(x => !string.IsNullOrEmpty(x)))
            {
                return await currentDirectory.TryGetItemAsync(directory) != null;
            }

            return true;
        }

        public async Task<string> ReadFile(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            return await FileIO.ReadTextAsync(file);
        }

        public async Task SaveFile(string directoryPath, string name, string contents)
        {
            if (!await DirectoryExists(directoryPath))
            {
                await createDirectory(directoryPath);
            }

            var directory = await StorageFolder.GetFolderFromPathAsync(directoryPath);
            var file = await directory.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, contents);
        }

        public async Task SaveFile(string directoryPath, string name, byte[] contents)
        {
            if (!await DirectoryExists(directoryPath))
            {
                await createDirectory(directoryPath);
            }

            var directory = await StorageFolder.GetFolderFromPathAsync(directoryPath);
            var file = await directory.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, contents);
        }

        public async Task CopyResourceFile(string resourceFilePath, string destinationDirectoryPath, string destinationFileName)
        {
            // Open file in application package
            var fileToRead = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{resourceFilePath}", UriKind.Absolute));
            
            if(! await DirectoryExists(destinationDirectoryPath))
            {
               await createDirectory(destinationDirectoryPath);
            }

            var destinationDirectory = await StorageFolder.GetFolderFromPathAsync(destinationDirectoryPath);
 
            var fileToWrite = await destinationDirectory.CreateFileAsync(destinationFileName, CreationCollisionOption.ReplaceExisting);

            byte[] buffer = new byte[1024];
            using (BinaryWriter fileWriter = new BinaryWriter(await fileToWrite.OpenStreamForWriteAsync()))
            {
                using (BinaryReader fileReader = new BinaryReader(await fileToRead.OpenStreamForReadAsync()))
                {
                    long readCount = 0;
                    while (readCount < fileReader.BaseStream.Length)
                    {
                        int read = fileReader.Read(buffer, 0, buffer.Length);
                        readCount += read;
                        fileWriter.Write(buffer, 0, read);
                    }
                }
            }

        }

        private async Task createDirectory(string path)
        {
            var subPath = path.Replace(StorageRoot, string.Empty);
            var subDirectories =  subPath.Split('\\');

            var current = StorageRoot;
            var currentDirectory = await StorageFolder.GetFolderFromPathAsync(StorageRoot);

            foreach(var directory in subDirectories.Where(x=> !string.IsNullOrEmpty(x)))
            {
                current = Path.Combine(current, directory);

                if (await currentDirectory.TryGetItemAsync(directory) == null)
                {
                   await currentDirectory.CreateFolderAsync(directory);
                }

                currentDirectory = await currentDirectory.GetFolderAsync(directory);
            }

        }

    }
}
