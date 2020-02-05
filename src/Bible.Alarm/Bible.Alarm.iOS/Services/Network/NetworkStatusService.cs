using Bible.Alarm.Contracts.Network;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.iOS.Services.Network
{
    public class NetworkStatusService : INetworkStatusService
    {
        public IContainer container { get; set; }
        public NetworkStatusService(IContainer container)
        {
            this.container = container;
        }

        public Task<bool> IsInternetAvailable()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }


    }
}