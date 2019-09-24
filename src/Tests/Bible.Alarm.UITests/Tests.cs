using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Bible.Alarm.UITests.Helpers;
using System.Threading;

namespace Bible.Alarm.UITests
{
    [TestFixture(Platform.Android)]
    //[TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void Smoke_Test_Alarm()
        {
            app.WaitForElement(c => c.Marked("HomePage"),
            "Took more than 10 seconds to show home page.", TimeSpan.FromSeconds(300));

            app.WaitForElement(c => c.Marked("AddScheduleButton"), 
                "Add Schedule Button is missing.", TimeSpan.FromSeconds(30));

            app.Screenshot("Home page.");

            app.Tap(x => x.Marked("AddScheduleButton"));
            app.WaitForElement(c => c.Marked("SchedulePage"),
            "Took more than 5 seconds to show add schedule page.", TimeSpan.FromSeconds(5));

            app.WaitForElement(c => c.Marked("CancelButton"),
                "Cancel Button is missing.", TimeSpan.FromSeconds(30));
            app.WaitForElement(c => c.Marked("SaveButton"), 
                "Save Button is missing.", TimeSpan.FromSeconds(30));

            var deviceTime = DateTime.Parse((string)app.Invoke("GetDeviceTime"));
            app.UpdateTimePicker(this.platform, deviceTime.Second <= 45 ? deviceTime.AddMinutes(1) : deviceTime.AddMinutes(2));

            app.Tap(x => x.Marked("SaveButton"));
            app.WaitForElement(c => c.Marked("HomePage"),
            "Took more than 5 seconds to show home page.", TimeSpan.FromSeconds(5));

            app.WaitForElement(c => c.Marked("AlarmModal"), "Alarm did not fire within time.", TimeSpan.FromMinutes(3));
            app.WaitForElement(c => c.Marked("AlarmDismissButton"), "Alarm dismiss button is missing.", TimeSpan.FromMinutes(3));

            var isAlarmOn = bool.Parse((string)app.Invoke("IsAlarmOn"));

            Assert.IsTrue(isAlarmOn, "Alarm is not On.");

            app.Tap(x => x.Marked("AlarmDismissButton"));

            app.WaitForElement(c => c.Marked("HomePage"),
           "Took more than 5seconds to show home page after alarm dismissal.", TimeSpan.FromSeconds(5));

            isAlarmOn = bool.Parse((string)app.Invoke("IsAlarmOn"));
            Assert.IsFalse(isAlarmOn, "Alarm is not off after dismissal.");
        }
    }
}
