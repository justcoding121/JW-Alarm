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
using JW.Alarm.Core.Uwp;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JW.Alarm.Core.UWP.Views.Music
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackSelection : Page
    {
        public TrackSelectionViewModel ViewModel => DataContext as TrackSelectionViewModel;

        public TrackSelection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter as TrackSelectionViewModel;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void MusicTrackListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SetTrack(e.ClickedItem as MusicTrackListViewItemModel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var listViewItem = (sender as Button).FindVisualAncestor<StackPanel>();

            var listViewItemViewModel = (listViewItem.DataContext as MusicTrackListViewItemModel);
            listViewItemViewModel.Play = !listViewItemViewModel.Play;
        }
    }
}
