using Android.App;
using Android.Media;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JW.Alarm.Services.Droid
{
    public class PreviewPlayService : Java.Lang.Object,
        MediaPlayer.IOnCompletionListener, IPreviewPlayService
    {
        private MediaPlayer player;

        public PreviewPlayService(MediaPlayer player)
        {
            this.player = player;
        }

        public event Action OnStopped;

        public void Play(string url)
        {
            var manifestUri = Android.Net.Uri.Parse(url);
            this.player.SetOnCompletionListener(this);
            this.player.SetDataSource(Application.Context, manifestUri);
            this.player.Start();
        }

        public void Stop()
        {
            player.Stop();
        }

        public void OnCompletion(MediaPlayer mp)
        {
            OnStopped?.Invoke();
        }
    }
}
