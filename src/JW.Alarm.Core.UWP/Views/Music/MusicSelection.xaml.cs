using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JW.Alarm.Core.UWP.Views.Music
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MusicSelection : Page
    {
        public MusicSelectionViewModel ViewModel => DataContext as MusicSelectionViewModel;

        public MusicSelection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter as MusicSelectionViewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void MusicSelectionListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var viewModel = ViewModel.GetBookSelectionViewModel(e.ClickedItem as MusicTypeListItemViewModel);

            if (viewModel.GetType() == typeof(SongBookSelectionViewModel))
            {
                Frame.Navigate(typeof(SongBookSelection), viewModel);
                return;
            }

            Frame.Navigate(typeof(TrackSelection), viewModel);
        }
    }
}
