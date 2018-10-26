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
    public class JsonDatabase : IDatabase
    {
        private IStorageService storageService;

        private string databasePath => Path.Combine(storageService.StorageRoot, "Database");
        private string tablePath<T>() where T : IEntity => Path.Combine(databasePath, tableName<T>());
        private string tableName<T>() where T : IEntity => typeof(T).Name;

        private ConcurrentDictionary<string, SemaphoreSlim> tableLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private ConcurrentDictionary<string, OrderedHashSet<int>> keyCache = new ConcurrentDictionary<string, OrderedHashSet<int>>();

        private AsyncLazy<OrderedHashSet<int>> keys<T>() where T : IEntity =>
                new AsyncLazy<OrderedHashSet<int>>(async () =>
                {
                    if (keyCache.TryGetValue(tableName<T>(), out var value))
                    {
                        return value;
                    }

                    var @lock = acquireTableLock<T>();
                    OrderedHashSet<int> keys;
                    try
                    {
                        await @lock.WaitAsync();
                        keys = new OrderedHashSet<int>(await readKeys<T>());
                        keyCache[tableName<T>()] = keys;
                    }
                    finally { @lock.Release(); }

                    return keys;
                });

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

        public JsonDatabase(IStorageService storageService)
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
            return (await keys<T>()).Count;
        }

        public async Task<T> Read<T>(int recordId) where T : IEntity
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
            if (record.Id > 0)
            {
                throw new ArgumentException("Object to insert already have an assigned primary key.", "Id");
            }

            record.Id = await getNextId<T>();
            var fileContent = JsonConvert.SerializeObject(record);
            await storageService.SaveFile(tablePath<T>(), $"{record.Id}.json", fileContent);
            var keys = await keys<T>();
            keys.Add(record.Id);
        }

        public async Task Update<T>(T record) where T : IEntity
        {
            if (record.Id == 0)
            {
                throw new ArgumentException("Invalid primary key.", "Id");
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

        public async Task Delete<T>(int recordId) where T : IEntity
        {
            if (recordId == 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            var @lock = acquireTableLock<T>();

            try
            {
                await @lock.WaitAsync();
                await storageService.DeleteFile(Path.Combine(tablePath<T>(), $"{recordId}.json"));
            }
            finally { @lock.Release(); }

            var keys = await keys<T>();
            keys.Remove(recordId);
        }

        private async Task<int> getNextId<T>() where T : IEntity
        {
            return (await keys<T>()).Max() + 1;
        }

        private async Task<IEnumerable<int>> readKeys<T>() where T : IEntity
        {
            var files = await storageService.GetAllFiles(tablePath<T>());

            return files.Where(x => x.EndsWith(".json"))
                        .Select(x => int.Parse(Path.GetFileNameWithoutExtension(x)));
        }
    }
}
