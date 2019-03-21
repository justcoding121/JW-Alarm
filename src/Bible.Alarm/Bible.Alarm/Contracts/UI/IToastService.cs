using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IToastService
    {
        Task ShowMessage(string message, int seconds = 3);
        Task ShowScheduledNotification(AlarmSchedule schedule, int seconds = 3);
    }
}
