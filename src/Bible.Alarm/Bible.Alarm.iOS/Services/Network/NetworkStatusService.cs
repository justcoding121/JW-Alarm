using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bible.Alarm.Contracts.Network;

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