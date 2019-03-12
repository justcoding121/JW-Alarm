using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace JW.Alarm.Services.UWP
{
    public class PreviewPlayService : IPreviewPlayService
    {
        private MediaPlayer mediaPlayer;

        public PreviewPlayService(MediaPlayer player)
        {
            this.mediaPlayer = player;
            mediaPlayer.MediaEnded += mediaEndHandler;
        }

        private void mediaEndHandler(MediaPlayer sender, object args)
        {
            OnStopped?.Invoke();
        }

        public event Action OnStopped;

        public void Play(string url)
        {
            var manifestUri = new Uri(url);
            mediaPlayer.Source = MediaSource.CreateFromUri(manifestUri);
            mediaPlayer.Play();
        }

        public void Stop()
        {
            mediaPlayer.Pause();
        }
    }
}
