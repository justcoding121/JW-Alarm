using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    //A simple json file database writing to file system.
    //Our simple requirements do not warrant the additional package weight and initialization cost of a real local database sqlite.
    public class JsonDatabase : IDatabase
    {
        private IStorageService storageService;

        private string databasePath => Path.Combine(storageService.StorageRoot, "Database");
        private string tablePath<T>() where T : IEntity => Path.Combine(databasePath, tableName<T>());
        private string tableName<T>() where T : IEntity => typeof(T).Name;

        private Random random = new Random();

        public JsonDatabase(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IEnumerable<T>> ReadAll<T>() where T : IEntity
        {
            var fileReadTasks = await storageService.GetAllFiles(tablePath<T>())
                .ContinueWith(m => m.Result.Select(file => storageService.ReadFile(file)
                .ContinueWith(z => JsonConvert.DeserializeObject<T>(z.Result))));

            return await Task.WhenAll(fileReadTasks);
        }

        public async Task<int> Count<T>() where T : IEntity
        {
            var files = await storageService.GetAllFiles(tablePath<T>());
            return files.Where(x => x.EndsWith(".json")).Count();
        }

        public async Task<T> Read<T>(int recordId) where T : IEntity
        {
            if (recordId == 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            var fileContent = await storageService.ReadFile(Path.Combine(tablePath<T>(), $"{recordId}.json"));
            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        public async Task Insert<T>(T record) where T : IEntity
        {
            if (record.Id > 0)
            {
                throw new ArgumentException("Object to insert already have an assigned primary key.", "Id");
            }

            record.Id = await getNextId<T>();
            var fileContent = JsonConvert.SerializeObject(record);
            await storageService.SaveFile(tablePath<T>(), $"{record.Id}.json", fileContent);
        }

        public async Task Update<T>(T record) where T : IEntity
        {
            if (record.Id == 0)
            {
                throw new ArgumentException("Invalid primary key.", "Id");
            }
            var fileContent = JsonConvert.SerializeObject(record);
            await storageService.SaveFile(tablePath<T>(), $"{record.Id}.json", fileContent);
        }

        public async Task Delete<T>(int recordId) where T : IEntity
        {
            if (recordId == 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            await storageService.DeleteFile(Path.Combine(tablePath<T>(), $"{recordId}.json"));
        }

        private async Task<int> getNextId<T>() where T : IEntity
        {
            var keys = new HashSet<int>(await readKeys<T>());

            var candidate = random.Next();

            while (keys.Contains(candidate))
            {
                candidate = random.Next();
            }

            return candidate;
        }

        private async Task<IEnumerable<int>> readKeys<T>() where T : IEntity
        {
            var files = await storageService.GetAllFiles(tablePath<T>());

            return files.Where(x => x.EndsWith(".json"))
                        .Select(x => int.Parse(Path.GetFileNameWithoutExtension(x)));
        }
    }
}
