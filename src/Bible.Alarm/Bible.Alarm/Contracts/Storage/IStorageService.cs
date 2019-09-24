using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IStorageService
    {
        string StorageRoot { get; }
        Task<bool> DirectoryExists(string path);
        Task<bool> FileExists(string path);
        Task<List<string>> GetAllFiles(string path);
        Task<DateTimeOffset> GetFileCreationDate(string path, bool isResourceFile);

        Task<string> ReadFile(string path);
        Task CopyResourceFile(string resourceFileName, string destinationDirectoryPath, string destinationFileName);
        Task SaveFile(string directoryPath, string fileName, string contents);
        Task SaveFile(string directoryPath, string fileName, byte[] contents);
        Task DeleteFile(string path);
        Task DeleteDirectory(string path);
    }
}
