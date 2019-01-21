using Advanced.Algorithms.DataStructures.Foundation;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    //A simple json file database writing to file system.
    //Our simple requirements do not warrant the additional package weight and initialization cost of a real local database sqlite.
    public class TableStorage : ITableStorage
    {
        private IStorageService storageService;

        private string databasePath => Path.Combine(storageService.StorageRoot, "Repository");
        private string tablePath<T>() where T : IEntity => Path.Combine(databasePath, tableName<T>());
        private string tableName<T>() where T : IEntity => typeof(T).Name;

        private ConcurrentDictionary<string, SemaphoreSlim> tableLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private ConcurrentDictionary<string, OrderedHashSet<long>> keyCache = new ConcurrentDictionary<string, OrderedHashSet<long>>();

        private SemaphoreSlim acquireTableLock<T>() where T : IEntity
        {
            while (true)
            {
                if (tableLocks.TryGetValue(tableName<T>(), out var value))
                {
                    return value;
                }

                var @lock = new SemaphoreSlim(1);
                if (tableLocks.TryAdd(tableName<T>(), @lock))
                {
                    return @lock;
                }
            }
        }

        public TableStorage(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IEnumerable<T>> ReadAll<T>() where T : IEntity
        {
            IEnumerable<T> result;
            var @lock = acquireTableLock<T>();

            try
            {
                await @lock.WaitAsync();

                var fileReadTasks = await storageService.GetAllFiles(tablePath<T>())
                    .ContinueWith(m => m.Result.Where(x => x.EndsWith(".json"))
                                        .Select(file => storageService.ReadFile(file)
                      .ContinueWith(z => JsonConvert.DeserializeObject<T>(z.Result))));

                result = await Task.WhenAll(fileReadTasks);
            }
            finally { @lock.Release(); }

            return result;
        }

        public async Task<int> Count<T>() where T : IEntity
        {
            return (await readKeys<T>()).Count();
        }

        public async Task<bool> Exists<T>(long recordId) where T : IEntity
        {
            return await storageService.FileExists(Path.Combine(tablePath<T>(), $"{recordId}.json"));
        }

        public async Task<T> Read<T>(long recordId) where T : IEntity
        {
            if (recordId == 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            string fileContent;
            var @lock = acquireTableLock<T>();
            try
            {
                await @lock.WaitAsync();
                fileContent = await storageService.ReadFile(Path.Combine(tablePath<T>(), $"{recordId}.json"));
            }
            finally { @lock.Release(); }

            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        public async Task Insert<T>(T record) where T : IEntity
        {
            if (record.Id != 0)
            {
                throw new ArgumentException("new record cannot have a primary key assigned.", "Id");
            }

            record.Id = getNextId();
            var fileContent = JsonConvert.SerializeObject(record);
            await storageService.SaveFile(tablePath<T>(), $"{record.Id}.json", fileContent);
        }

        public async Task Update<T>(T record) where T : IEntity
        {
            if (record.Id <= 0)
            {
                throw new ArgumentException("Invalid primary key.", "Id");
            }

            if(!await Exists<T>(record.Id))
            {
                throw new Exception("Record does not exist.");
            }

            var fileContent = JsonConvert.SerializeObject(record);
            var @lock = acquireTableLock<T>();

            try
            {
                await @lock.WaitAsync();
                await storageService.SaveFile(tablePath<T>(), $"{record.Id}.json", fileContent);
            }
            finally { @lock.Release(); }
        }

        public async Task Delete<T>(long recordId) where T : IEntity
        {
            if (recordId <= 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            if (!await Exists<T>(recordId))
            {
                throw new Exception("Record does not exist.");
            }

            var @lock = acquireTableLock<T>();

            try
            {
                await @lock.WaitAsync();
                await storageService.DeleteFile(Path.Combine(tablePath<T>(), $"{recordId}.json"));
            }
            finally { @lock.Release(); }
        }

        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        private long getNextId()
        {
            long original, newValue;
            do
            {
                original = lastTimeStamp;
                long now = DateTime.UtcNow.Ticks;
                newValue = Math.Max(now, original + 1);
            } while (Interlocked.CompareExchange(ref lastTimeStamp, newValue, original) != original);

            return newValue;
        }

        private async Task<IEnumerable<long>> readKeys<T>() where T : IEntity
        {
            var files = await storageService.GetAllFiles(tablePath<T>());

            return files.Where(x => x.EndsWith(".json"))
                        .Select(x => long.Parse(Path.GetFileNameWithoutExtension(x)));
        }
    }
}
