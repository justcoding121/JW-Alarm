using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.Music;
using Bible.Alarm;
using Bible.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Bible.Alarm.UI.Views.General;

namespace Bible.Alarm.UI
{
    public static class IocSetup
    {
        internal static IContainer Container { private set; get; }
        public static void Initialize(IContainer container)
        {
            Container = container;

            Container.Register(x => new Schedule());

            Container.Register(x => new MusicSelection());
            Container.Register(x => new SongBookSelection());
            Container.Register(x => new TrackSelection());

            Container.Register(x => new BibleSelection());
            Container.Register(x => new BookSelection());
            Container.Register(x => new ChapterSelection());

            Container.Register(x => new LanguageModal());

            Container.Register(x => new AlarmModal());
            Container.Register(x => new BatteryOptimizationExclusionModal());
            Container.Register(x => new NumberOfChaptersModal());
            Container.Register(x => new MediaProgressModal());
        }
    }
}
