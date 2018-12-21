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

namespace JW.Alarm.Core.UWP.Views.Shared
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LanguageSelection : UserControl
    {
        public LanguageSelection()
        {
            this.InitializeComponent();
            this.LayoutUpdated += LanguageSelection_LayoutUpdated;
        }

        private void LanguageSelection_LayoutUpdated(object sender, object e)
        {
            if (ActualWidth == 0 && ActualHeight == 0)
            {
                return;
            }

            this.LanguageListView.MaxHeight = Window.Current.Bounds.Height - 200;
        }

        private void LanguageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                // in this example we assume the parent of the UserControl is a Popup 
                Popup p = this.Parent as Popup;

                // close the Popup
                if (p != null) { p.IsOpen = false; }
            }
        }
    }
}
