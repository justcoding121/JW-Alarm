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

        public void Stop()
        {
            player.Stop();
        }

        public void OnCompletion(MediaPlayer mp)
        {
            OnStopped?.Invoke();
        }

        async Task IPreviewPlayService.Play(string url)
        {
            await Task.Delay(500);
            await Task.Run(() =>
            {
                var uri = Android.Net.Uri.Parse(url);
                this.player.Reset();
                this.player.SetOnCompletionListener(this);
                this.player.SetDataSource(IocSetup.Context, uri);
                this.player.Prepare();
                this.player.Start();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.player.Dispose();
        }
    }
}
