using Bible.Alarm.Common.DataStructures;
using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationDetail>> Notifications { get; }

        Task<NotificationDetail> Read(long playDetailId);
        Task Add(NotificationDetail playDetail);
        Task Update(NotificationDetail playDetail);
        Task Remove(long playDetailId);
    }
}
