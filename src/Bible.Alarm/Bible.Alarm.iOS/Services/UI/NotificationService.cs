using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Helpers;
using Bible.Alarm.iOS.Models;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Models.Schedule;
using Bible.Alarm.Services.Contracts;
using Foundation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private readonly IContainer container;
        private readonly ScheduleDbContext dbContext;

        public iOSNotificationService(IContainer container, ScheduleDbContext dbContext)
        {
            this.container = container;
            this.dbContext = dbContext;
        }

        public void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            _ = iosAlarmHandler.Handle(scheduleId);
        }

        public Task ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            return Task.CompletedTask;

        }

        public Task Remove(long scheduleId)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsScheduled(long scheduleId)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }

}
