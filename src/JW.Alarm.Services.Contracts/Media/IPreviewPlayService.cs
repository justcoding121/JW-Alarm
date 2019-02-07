using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Services.Contracts
{
    public interface IPreviewPlayService
    {
        void Play(string url);
        void Stop();
    }
}
