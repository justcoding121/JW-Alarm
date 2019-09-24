using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPreviewPlayService : IDisposable
    {
        Task Play(string url);
        void Stop();
        event Action OnStopped;
    }
}
