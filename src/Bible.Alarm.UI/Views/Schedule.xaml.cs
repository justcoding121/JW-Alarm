using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Command = new Command(() => animateButtonTouched(Music_Btn, 1500, GetHexString(Color.LightGray), GetHexString(Color.WhiteSmoke), 1))
            });

            Bible_Btn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => animateButtonTouched(Bible_Btn, 1500, GetHexString(Color.LightGray), GetHexString(Color.WhiteSmoke), 1))
            });

        }

        public string GetHexString(Color color)
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

        private async void Button_Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void Button_Save_Clicked(object sender, EventArgs e)
        {
            if (await ViewModel.SaveAsync())
            {
                await Navigation.PopAsync();
            }
        }

        private void Button_Day_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            switch (button.Text)
            {
                case "S":
                    ViewModel.Toggle(DaysOfWeek.Sunday);
                    break;
                case "M":
                    ViewModel.Toggle(DaysOfWeek.Monday);
                    break;
                case "T":
                    ViewModel.Toggle(DaysOfWeek.Tuesday);
                    break;
                case "W":
                    ViewModel.Toggle(DaysOfWeek.Wednesday);
                    break;
                    //case "Button_Thursday":
                    //    ViewModel.Toggle(DaysOfWeek.Thursday);
                    //    break;
                    //case "Button_Friday":
                    //    ViewModel.Toggle(DaysOfWeek.Friday);
                    //    break;
                    //case "Button_Saturday":
                    //    ViewModel.Toggle(DaysOfWeek.Saturday);
                    //    break;
            }
        }


        private async void Button_Delete_Clicked(object sender, EventArgs e)
        {
            await ViewModel.DeleteAsync();
            await Navigation.PopAsync();
        }

        private void TapGestureRecognizer_Music_Tapped(object sender, EventArgs e)
        {

        }

        private void TapGestureRecognizer_Bible_Tapped(object sender, EventArgs e)
        {

        }
    }
}