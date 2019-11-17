using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Contracts.Network
{
    public interface INetworkStatusService : IDisposable
    {
        Task<bool> IsInternetAvailable();
    }
}
