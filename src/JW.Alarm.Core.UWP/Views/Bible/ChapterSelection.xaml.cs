using JW.Alarm.Core.Uwp;
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

namespace JW.Alarm.Core.UWP.Views.Bible
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChapterSelection : Page
    {
        public ChapterSelectionViewModel ViewModel => DataContext as ChapterSelectionViewModel;

        public ChapterSelection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter as ChapterSelectionViewModel;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BibleChapterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SetChapter(e.ClickedItem as BibleChapterListViewItemModel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var listViewItem = (sender as Button).FindVisualAncestor<StackPanel>();

            var listViewItemViewModel = (listViewItem.DataContext as BibleChapterListViewItemModel);
            listViewItemViewModel.Play = !listViewItemViewModel.Play;
        }
    }
}
