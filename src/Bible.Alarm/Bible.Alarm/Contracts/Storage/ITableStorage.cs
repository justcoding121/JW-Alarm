using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface ITableStorage : IDisposable
    {
        Task<IEnumerable<T>> ReadAll<T>() where T : IEntity;
        Task<int> Count<T>() where T : IEntity;
        Task<bool> Exists<T>(long Id) where T : IEntity;
        Task<T> Read<T>(long recordId) where T : IEntity;
        Task Insert<T>(T record) where T : IEntity;
        Task Update<T>(T record) where T : IEntity;
        Task Delete<T>(long recordId) where T : IEntity;
    }
}
