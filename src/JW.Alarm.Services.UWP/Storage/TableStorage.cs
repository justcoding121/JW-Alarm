using Advanced.Algorithms.DataStructures.Foundation;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Uwp
{

    //A simple json file database writing to file system.
    //Our simple requirements do not warrant the additional package weight and initialization cost of a real local database sqlite.
    public class TableStorage : ITableStorage
    {
        private static string dbName = "bibleAlarm.db";

        private static string tableName<T>() => typeof(T).Name;

        public async Task<IEnumerable<T>> ReadAll<T>() where T : IEntity
        {
            var result = new List<T>();

            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT key, value from {tableName<T>()}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (await query.ReadAsync())
                {
                    var key = query.GetInt64(0);
                    var value = JsonConvert.DeserializeObject<T>(query.GetString(1));
                    value.Id = key;

                    result.Add(value);
                }

                db.Close();
            }

            return result;
        }

        public async Task<int> Count<T>() where T : IEntity
        {
            return (await readKeys<T>()).Count();
        }

        public async Task<bool> Exists<T>(long recordId) where T : IEntity
        {
            var result = false;

            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT id from {tableName<T>()} where id={recordId};", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                if (await query.ReadAsync())
                {
                    result = true;
                }

                db.Close();
            }

            return result;
        }

        public async Task<T> Read<T>(long recordId) where T : IEntity
        {
            if (recordId == 0)
            {
                throw new ArgumentException("Invalid primary key.", "recordId");
            }

            T result = default(T);

            using (SqliteConnection db =
                 new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT id from {tableName<T>()} where id=@recordId;", db);      
                selectCommand.Parameters.AddWithValue("@recordId", recordId);
                selectCommand.Connection = db;

                SqliteDataReader query = selectCommand.ExecuteReader();

                if (await query.ReadAsync())
                {
                    var key = query.GetInt64(0);
                    var e = query.GetValue(1);
                    var m = query.GetString(1);
                    var value = JsonConvert.DeserializeObject<T>(m);
                    value.Id = key;

                    result = value;
                }

                db.Close();
            }

            return result;
        }

        public async Task Insert<T>(T record) where T : IEntity
        {
            if (record.Id != 0)
            {
                throw new ArgumentException("new record cannot have a primary key assigned.", "Id");
            }

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"INSERT INTO {tableName<T>()} VALUES (NULL, @Entry);";
                insertCommand.Parameters.AddWithValue("@Entry", JsonConvert.SerializeObject(record));

                await insertCommand.ExecuteNonQueryAsync();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"select last_insert_rowid();", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                if (await query.ReadAsync())
                {
                    record.Id = query.GetInt64(0);
                }

                db.Close();
            }

        }

        public async Task Update<T>(T record) where T : IEntity
        {
            if (record.Id <= 0)
            {
                throw new ArgumentException("Invalid primary key.", "Id");
            }

            if (!await Exists<T>(record.Id))
            {
                throw new Exception("Record does not exist.");
            }

            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();
            
                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                updateCommand.CommandText = $"UPDATE {tableName<T>()} SET data=@Entry WHERE id={record.Id};";
                updateCommand.Parameters.AddWithValue("@Entry", JsonConvert.SerializeObject(record));

                await updateCommand.ExecuteNonQueryAsync();

                db.Close();
            }

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


            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"DELETE FROM {tableName<T>()} WHERE id={recordId};";
                await insertCommand.ExecuteNonQueryAsync();

                db.Close();
            }
        }


        private async Task<IEnumerable<long>> readKeys<T>() where T : IEntity
        {
            var result = new List<long>();

            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT id from {tableName<T>()};", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (await query.ReadAsync())
                {
                    var key = query.GetInt64(0);
                    result.Add(key);
                }

                db.Close();
            }

            return result;

        }
    }
}
