﻿using Bible.Alarm.UI.ViewHelpers;
using Bible.Alarm.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views
{
    public partial class Schedule : ContentPage
    {
        public ScheduleViewModel ViewModel => BindingContext as ScheduleViewModel;

        public Schedule()
        {
            InitializeComponent();

            MusicButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => AnimateUtils.FlickUponTouched(MusicButton, 1500,
                    ColorUtils.ToHexString(Color.LightGray), ColorUtils.ToHexString(Color.WhiteSmoke), 1))
            });

            BibleButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => AnimateUtils.FlickUponTouched(BibleButton, 1500,
                    ColorUtils.ToHexString(Color.LightGray), ColorUtils.ToHexString(Color.WhiteSmoke), 1))
            });
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.CancelCommand.Execute(null);
            return true;
        }
    }
}