using Android.Content;
using Android.Net;
using Android.OS;
using Bible.Alarm.Contracts.Network;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid.Services.Network
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
            var connectivityManager = (ConnectivityManager)container.AndroidContext().GetSystemService(Context.ConnectivityService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var networkInfo = connectivityManager.ActiveNetwork;

                if (networkInfo == null)
                {
                    return Task.FromResult(false);
                }

                var capabilities = connectivityManager.GetNetworkCapabilities(networkInfo);

                return Task.FromResult(capabilities != null
                    && (capabilities.HasTransport(TransportType.Wifi)
                    || capabilities.HasTransport(TransportType.Cellular)));
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        public void Dispose()
        {

        }


    }
}