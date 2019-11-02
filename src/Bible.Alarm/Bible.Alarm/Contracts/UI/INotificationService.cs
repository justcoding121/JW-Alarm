using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface INotificationService
    {
        void Add(long scheduleId, DateTimeOffset time, string title, string body);
        void Remove(long scheduleId);
        bool IsScheduled(long scheduleId);

        void ClearAll();
    }
}
