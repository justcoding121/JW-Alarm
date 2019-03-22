using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.Music;
using JW.Alarm;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            Container = container;

            Container.Register<INavigationService>(x => new NavigationService(container.Resolve<INavigation>()), isSingleton: true);

            Container.Register(x => new Schedule());

            Container.Register(x => new MusicSelection());
            Container.Register(x => new SongBookSelection());
            Container.Register(x => new TrackSelection());

            Container.Register(x => new BibleSelection());
            Container.Register(x => new BookSelection());
            Container.Register(x => new ChapterSelection());

            Container.Register(x => new LanguageModal());
        }
    }
}
