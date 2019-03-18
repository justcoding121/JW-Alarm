using AVFoundation;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JW.Alarm.Services.iOS
{
    public class PreviewPlayService :  IPreviewPlayService
    {
        private AVPlayer player;

        public PreviewPlayService(AVPlayer player)
        {
            this.player = player;
        }

        public event Action OnStopped;

        public void Play(string url)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    
    }
}
