using Bible.Alarm.Services;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace JW.Alarm.Services.UWP
{
    public class UwpToastService : ToastService
    {
        public override Task ShowMessage(string message, int seconds)
        {
            var flyout = new Flyout();

            flyout.Content = new TextBlock()
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };

            flyout.Placement = FlyoutPlacementMode.Bottom;

            Frame currentFrame = Window.Current.Content as Frame;

            flyout.OverlayInputPassThroughElement = currentFrame;

            flyout.ShowAt(currentFrame);

            var hideTask = Task.Delay(seconds * 1000).ContinueWith(x =>
            {
                flyout.Hide();

            }, TaskScheduler.FromCurrentSynchronizationContext());

            return Task.FromResult(false);
        }
    }
}
