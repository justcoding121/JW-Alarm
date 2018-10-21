using JW.Alarm.Services.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace JW.Alarm.Services.UWP
{
    public class UwpThreadService : IThreadService
    {
        public async Task RunOnUIThread(Action action)
        {
            //already in UI thread
            if(CoreWindow.GetForCurrentThread().Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    action();
                });
            }

           
        }
    }
}
