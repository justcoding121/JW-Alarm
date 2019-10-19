using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bible.Alarm.Contracts.Network;

namespace Bible.Alarm.Droid.Services.Network
{
    public class NetworkStatusService : INetworkStatusService
    {
        public Task<bool> IsInternetAvailable()
        {
            var connectivityManager = (ConnectivityManager)IocSetup.Context.GetSystemService(Android.Content.Context.ConnectivityService);
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
    }
}