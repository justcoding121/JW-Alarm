using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IPopUpService
    {
        Task ShowProgressRing();
        Task HideProgressRing();
        Task ShowMessage(string message, int seconds = 3);
        Task ShowScheduledNotification(AlarmSchedule schedule, int seconds = 3);
    }
}
