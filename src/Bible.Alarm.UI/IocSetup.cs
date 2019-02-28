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

            Container.Register(x => new Home()
            {
                BindingContext = container.Resolve<HomeViewModel>()
            }, isSingleton: true);

            Container.Register<INavigationService>(x => new NavigationService(container.Resolve<INavigation>()), isSingleton: true);

            Container.Register(x => new Schedule(), isSingleton: true);

            Container.Register(x => new MusicSelection(), isSingleton: true);
            Container.Register(x => new SongBookSelection(), isSingleton: true);
            Container.Register(x => new TrackSelection(), isSingleton: true);

            Container.Register(x => new BibleSelection(), isSingleton: true);
            Container.Register(x => new BookSelection(), isSingleton: true);
            Container.Register(x => new ChapterSelection(), isSingleton: true);

            Container.Register(x => new LanguageModal(), isSingleton: true);
        }
    }
}
