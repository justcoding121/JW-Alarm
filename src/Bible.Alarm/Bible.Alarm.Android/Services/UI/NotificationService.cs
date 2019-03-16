using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace JW.Alarm.Services.UWP
{
    public class DroidNotificationService : INotificationService
    {
        IMediaCacheService mediaCacheService;

        public DroidNotificationService(IMediaCacheService mediaCacheService)
        {
            this.mediaCacheService = mediaCacheService;
        }

        public void Add(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            throw new NotImplementedException();
        }


        public void Remove(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public bool IsScheduled(long scheduleId)
        {
            throw new NotImplementedException();
        }
    }

}
