using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IThreadService
    {
        Task RunOnUIThread(Action action);
    }
}
