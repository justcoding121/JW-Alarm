using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Views;
using Android.Widget;

namespace Bible.Alarm.Droid.Services.Media
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "android.media.browse.MediaBrowserService" })]
    public class MusicService : MediaBrowserServiceCompat
    {
        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            throw new NotImplementedException();
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            throw new NotImplementedException();
        }
    }
}