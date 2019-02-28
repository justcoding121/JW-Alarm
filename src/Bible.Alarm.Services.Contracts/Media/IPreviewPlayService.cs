using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IPreviewPlayService
    {
        void Play(string url);
        void Stop();
        event Action OnStopped;
    }
}
