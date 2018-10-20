using JW.Alarm.Services.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace JW.Alarm.Services.UWP
{
    public class UwpThreadService : IThreadService
    {
        public async Task RunOnUIThread(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                action();
            });
        }
    }
}
