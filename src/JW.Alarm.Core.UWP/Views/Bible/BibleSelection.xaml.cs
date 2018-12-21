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
    public sealed partial class BibleSelection : Page
    {
        public BibleSelectionViewModel ViewModel => DataContext as BibleSelectionViewModel;

        public BibleSelection()
        {
            this.InitializeComponent();
        }

        private void LanguagePopup_LayoutUpdated(object sender, object e)
        {
            if (LanguagePopup.IsOpen)
            {
                if (LanguagePopupContent.ActualWidth == 0 && LanguagePopupContent.ActualHeight == 0)
                {
                    return;
                }

                double ActualHorizontalOffset = this.LanguagePopup.HorizontalOffset;
                double ActualVerticalOffset = this.LanguagePopup.VerticalOffset;

                double NewHorizontalOffset = (Window.Current.Bounds.Width - LanguagePopupContent.ActualWidth) / 2;
                double NewVerticalOffset = (Window.Current.Bounds.Height - LanguagePopupContent.ActualHeight) / 2;

                if (ActualHorizontalOffset != NewHorizontalOffset || ActualVerticalOffset != NewVerticalOffset)
                {
                    this.LanguagePopup.HorizontalOffset = NewHorizontalOffset;
                    this.LanguagePopup.VerticalOffset = NewVerticalOffset;
                }
            }
        }

        private void BibleSelection_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var transform = Window.Current.Content.TransformToVisual(LanguagePopup);
            Point point = transform.TransformPoint(new Point(0, 0)); 

            double hOffset = (Window.Current.Bounds.Width - this.ActualWidth) / 2;
            double vOffset = (Window.Current.Bounds.Height - this.ActualHeight) / 2;

            LanguagePopup.HorizontalOffset = point.X + hOffset;
            LanguagePopup.VerticalOffset = point.Y + vOffset;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter as BibleSelectionViewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BibleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            if (!LanguagePopup.IsOpen) { LanguagePopup.IsOpen = true; }
        }
    }
}
