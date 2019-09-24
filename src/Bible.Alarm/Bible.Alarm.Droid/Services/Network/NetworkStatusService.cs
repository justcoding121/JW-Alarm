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
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var networkInfo = connectivityManager.ActiveNetworkInfo;
            return Task.FromResult(networkInfo != null && networkInfo.IsConnected);
        }
    }
}