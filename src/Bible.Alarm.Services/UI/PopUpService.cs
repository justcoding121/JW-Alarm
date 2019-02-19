using JW.Alarm.Common.Mvvm;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services
{
    public abstract class PopUpService : IPopUpService
    {
        private readonly IThreadService threadService;

        public PopUpService(IThreadService threadService)
        {
            this.threadService = threadService;
        }

        public abstract Task ShowMessage(string message, int seconds = 3);

        public async Task ShowScheduledNotification(AlarmSchedule schedule, int seconds = 3)
        {
            var nextFire = schedule.NextFireDate();
            var timeSpan = nextFire - DateTimeOffset.Now;
            await ShowMessage($"Alarm set for {timeSpan.Hours} hours and {timeSpan.Minutes} minutes from now.");
        }
    }
}
