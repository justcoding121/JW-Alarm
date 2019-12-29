using Bible.Alarm.Services.Contracts;
using System;
using System.Threading.Tasks;


namespace Bible.Alarm.Services.iOS
{
    public class PreviewPlayService : IPreviewPlayService, IDisposable
    {
        private IContainer container;

        public PreviewPlayService(IContainer container)
        {
            this.container = container;
        }

        public event Action OnStopped;

        public void Stop()
        {
            throw new NotImplementedException();
        }


        Task IPreviewPlayService.Play(string url)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
         
        }
    }
}
