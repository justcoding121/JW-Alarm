[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Xamarin.Forms.Switch),
    typeof(Bible.Alarm.Droid.CustomRederers.CustomSwitchRenderer))]
namespace Bible.Alarm.Droid.CustomRederers
{
    public class CustomSwitchRenderer : Xamarin.Forms.Platform.Android.SwitchRenderer
    {
        public CustomSwitchRenderer(Android.Content.Context context)
            : base(context) { }

        protected override Android.Widget.Switch CreateNativeControl()
        {
            return new Android.Widget.Switch(
                new Android.Views.ContextThemeWrapper(
                    this.Context,
                    Resource.Style.MyTheme_Switch /* <- Custom Switch Style */));
        }
    }
}