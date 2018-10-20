using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace JW.Alarm.Services.UWP
{
    public class UwpPopUpService : IPopUpService
    {
        private IThreadService threadService;

        public UwpPopUpService(IThreadService threadService)
        {
            this.threadService = threadService;
        }

        public async Task ShowMessage(string message, int seconds)
        {
            var flyout = new Flyout();
            flyout.Content = new TextBlock()
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };

            flyout.Placement = FlyoutPlacementMode.Bottom;

            Frame currentFrame = Window.Current.Content as Frame;

            await threadService.RunOnUIThread(() => flyout.ShowAt(currentFrame));

            var hideTask = Task.Delay(seconds * 1000).ContinueWith(async x =>
            {
                await threadService.RunOnUIThread(() => flyout.Hide());
            });

        }
    }
}
