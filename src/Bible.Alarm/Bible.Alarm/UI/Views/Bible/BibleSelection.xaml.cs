﻿using Bible.Alarm.UI.ViewHelpers;
using Bible.Alarm.ViewModels;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Bible
{
    public partial class BibleSelection : ContentPage
    {
        public BibleSelectionViewModel ViewModel => BindingContext as BibleSelectionViewModel;

        public BibleSelection()
        {
            InitializeComponent();

            BackButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => AnimateUtils.FlickUponTouched(BackButton, 1500,
                ColorUtils.ToHexString(Color.LightGray), ColorUtils.ToHexString(Color.WhiteSmoke), 1))
            });
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.BackCommand.Execute(null);
            return true;
        }
    }
}