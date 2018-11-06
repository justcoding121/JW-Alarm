using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Services
{
    public interface INotificationService
    {
        void Add(string groupName, DateTimeOffset notificationTime, string title, string body, string audioUrl);
        bool Remove(string groupName);
        bool IsScheduled(string groupName);
        void Clear();
    }
}
