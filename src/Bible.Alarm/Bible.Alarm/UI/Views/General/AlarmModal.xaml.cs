﻿using Bible.Alarm.ViewModels;

using Xamarin.Forms;

namespace Bible.Alarm.UI.Views
{
    public partial class AlarmModal : ContentPage
    {
        public AlarmViewModal ViewModel => BindingContext as AlarmViewModal;
        public AlarmModal()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.DismissCommand.Execute(null);
            return true;
        }

    }
}