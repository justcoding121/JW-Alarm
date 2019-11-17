using Android.App;
using Android.Media;
using Bible.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bible.Alarm.Services.Droid
{
    public class PreviewPlayService : Java.Lang.Object,
        MediaPlayer.IOnCompletionListener, IPreviewPlayService, IDisposable
    {
        private MediaPlayer player;

        public PreviewPlayService(MediaPlayer player)
        {
            this.player = player;
        }

        public event Action OnStopped;

        public void Stop()
        {
            player.Stop();
        }

        public void OnCompletion(MediaPlayer mp)
        {
            OnStopped?.Invoke();
        }

        Task IPreviewPlayService.Play(string url)
        {
            var uri = Android.Net.Uri.Parse(url);
            this.player.Reset();
            this.player.SetOnCompletionListener(this);
            this.player.SetDataSource(IocSetup.Context, uri);
            this.player.Prepare();
            this.player.Start();

            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.player.Dispose();
        }
    }
}
