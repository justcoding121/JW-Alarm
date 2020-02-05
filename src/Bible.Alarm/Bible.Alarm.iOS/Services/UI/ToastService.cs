using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS
{
    public class iOSToastService : ToastService, IDisposable
    {
        private IContainer container;
        public iOSToastService(IContainer container)
        {
            this.container = container;
        }
        public override Task ShowMessage(string message, int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
