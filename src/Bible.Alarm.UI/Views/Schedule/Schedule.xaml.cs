using JW.Alarm.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Schedule : ContentPage
    {
        public ScheduleViewModel ViewModel => BindingContext as ScheduleViewModel;

        public Schedule()
        {
            InitializeComponent();

            Music_Btn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => animateButtonTouched(Music_Btn, 1500, getHexString(Color.LightGray), getHexString(Color.WhiteSmoke), 1))
            });

            Bible_Btn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => animateButtonTouched(Bible_Btn, 1500, getHexString(Color.LightGray), getHexString(Color.WhiteSmoke), 1))
            });

        }

        private string getHexString(Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hex = $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

            return hex;
        }

        private void animateButtonTouched(View view, uint duration, string hexColorInitial, string hexColorFinal, int repeatCountMax)
        {
            var repeatCount = 0;
            view.Animate("changedBG", new Animation((val) =>
            {
                if (repeatCount == 0)
                {
                    view.BackgroundColor = Color.FromHex(hexColorInitial);
                }
                else
                {
                    view.BackgroundColor = Color.FromHex(hexColorFinal);
                }
            }), duration, finished: (val, b) =>
            {
                repeatCount++;
            }, repeat: () =>
            {
                return repeatCount < repeatCountMax;
            });
        }
    }
}