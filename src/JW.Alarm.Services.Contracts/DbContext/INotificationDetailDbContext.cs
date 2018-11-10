using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public interface INotificationDetailDbContext
    {
        Task<ObservableDictionary<long, NotificationDetail>> PlayDetails { get; }

        Task<NotificationDetail> Read(long playDetailId);
        Task Add(NotificationDetail playDetail);
        Task Update(NotificationDetail playDetail);
        Task Remove(long playDetailId);
    }
}
