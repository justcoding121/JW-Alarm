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
            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                var taskCompletionSource = new TaskCompletionSource<bool>();

                await CoreApplication.MainView.CoreWindow.Dispatcher
                        .RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                action();
                                taskCompletionSource.SetResult(true);
                            }
                            catch (Exception ex)
                            {
                                taskCompletionSource.SetException(ex);
                            }
                        });

                await taskCompletionSource.Task;
            }

        }
    }
}
