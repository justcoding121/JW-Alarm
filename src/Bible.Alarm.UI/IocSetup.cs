using Bible.Alarm.Services.Contracts;
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
        }
    }
}
