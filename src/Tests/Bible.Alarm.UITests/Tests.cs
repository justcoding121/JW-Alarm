using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

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
        public void HomeScreenIsDisplayed()
        {
            AppResult[] results = app.WaitForElement(c =>
            {
                return c.Marked("AddScheduleButton");
            },
            "Took too long to show home page.", TimeSpan.FromSeconds(10));

            app.Screenshot("Home page.");

            Assert.IsTrue(results.Any());
        }
    }
}
