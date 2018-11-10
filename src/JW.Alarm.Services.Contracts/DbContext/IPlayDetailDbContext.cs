using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public interface IPlayDetailDbContext
    {
        Task<ObservableDictionary<int, PlayDetail>> PlayDetails { get; }

        Task<PlayDetail> Read(int playDetailId);
        Task Create(PlayDetail playDetail);
        Task Update(PlayDetail playDetail);
        Task Delete(int playDetailId);
    }
}
