using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IDatabase
    {
        Task<IEnumerable<T>> ReadAll<T>() where T : IEntity;
        Task<int> Count<T>() where T : IEntity;
        Task<T> Read<T>(int recordId) where T : IEntity;
        Task Insert<T>(T record) where T : IEntity;
        Task Update<T>(T record) where T : IEntity;
        Task Delete<T>(int recordId) where T : IEntity;
    }

}
