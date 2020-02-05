﻿using System;
using Xamarin.UITest;

namespace Bible.Alarm.UITests.Helpers
{
    /// <summary>
    /// This extension class extends the <see cref="IApp"/> interface to add extension methods for selecting values from the default Date/Time pickers in iOS and Android.
    /// </summary>
    public static class AppPickerExtensions
    {
        /// <summary>
        /// Gets the standard timeout period.
        /// </summary>
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        #region Android Configuration
        private const string AndroidDatePickerClass = "DatePicker";
        private const string AndroidTimePickerClass = "TimePicker";
        /// <summary>
        /// Gets the Android Set Minute method for the TimePicker class. As of API v 23 this is deprecated and should be replaced with "setMinute".
        /// </summary>
        private const string AndroidTimePickerSetMinuteMethod = "setCurrentMinute";
        /// <summary>
        /// Gets the Android Set Hour method for the TimePicker class. As of API v 23 this is deprecated and should be replaced with "setHour".
        /// </summary>
        private const string AndroidTimePickerSetHourMethod = "setCurrentHour";
        #endregion

        #region iOS Configuration
        private const string iOSDatePickerClass = "UIDatePicker";
        private const string iOSTableViewClass = "UIPickerTableView";

        private const int iOSTimeHourColumn = 0;
        private const int iOSTimeMinuteColumn = 3;
        private const int iOSTimePeriodColumn = 6;
        #endregion
        /// <summary>
        /// Updates the presented time picker with the time provided.
        /// Does not request or dismiss the picker; you will need to do so yourself.
        /// </summary>
        /// <param name="app">App.</param>
        /// <param name="platform">Platform being tested.</param>
        /// <param name="time">Time to be selected. Date component is ignored.</param>
        /// <param name="pickerClass">Android picker class if subclassed.</param>
        /// <param name="timeout">Timeout for picker scrolling.</param>
        public static void UpdateTimePicker(this IApp app, Platform platform, DateTime time, string pickerClass = AndroidTimePickerClass, TimeSpan? timeout = null)
        {
            var hours = time.Hour;
            var minute = time.Minute;
            if (platform == Platform.Android)
            {
                app.WaitForElement(x => x.Marked("12:00 AM"), timeout: DefaultTimeout);
                app.Tap(x => x.Marked("12:00 AM"));

                app.Screenshot("Timepicker modal.");

                app.WaitForElement(x => x.Class(AndroidTimePickerClass), timeout: DefaultTimeout);
                app.Query(c => c.Class(pickerClass).Invoke(AndroidTimePickerSetHourMethod, hours));
                app.Query(c => c.Class(pickerClass).Invoke(AndroidTimePickerSetMinuteMethod, minute));

                app.Tap(c => c.Marked("OK"));
            }
            else if (platform == Platform.iOS)
            {
                // Assumes 12 Hour Clock
                app.WaitForElement(x => x.Class(iOSDatePickerClass), timeout: DefaultTimeout);
                var period = time.Hour >= 12 ? "PM" : "AM";
                ScrollToPickerColumn(app, iOSTimeHourColumn, time.ToString("h"), timeout);
                ScrollToPickerColumn(app, iOSTimeMinuteColumn, time.ToString("m"), timeout);
                ScrollToPickerColumn(app, iOSTimePeriodColumn, period, timeout);
            }
        }

        /// <summary>
        /// Scrolls to the designated picker column (for iOS only).
        /// </summary>
        /// <param name="app">App instance.</param>
        /// <param name="columnIndex">Column index to scroll to.</param>
        /// <param name="marked">Marked content to scroll to.</param>
        /// <param name="timeout">Timeout for scrolling.</param>
        private static void ScrollToPickerColumn(IApp app, int columnIndex, string marked, TimeSpan? timeout = null)
        {
            timeout = timeout ?? DefaultTimeout;
            app.ScrollDownTo(z => z.Marked(marked), x => x.Class(iOSTableViewClass).Index(columnIndex), timeout: timeout, strategy: ScrollStrategy.Auto);
            app.Tap(x => x.Text(marked));
        }
    }
}
