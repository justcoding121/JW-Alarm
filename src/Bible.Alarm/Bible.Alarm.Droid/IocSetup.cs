using Android.App;
using Android.Content;
using Bible.Alarm;
using MediaManager;

namespace Bible.Alarm.Droid
{

    public static class IocSetup
    {
        public static IContainer Container { get; private set; }
        public static Context Context { get; private set; }

        private static object @lock = new object();

        public static bool Initialize(Context context, bool isService)
        {
            lock (@lock)
            {
              
                if (Container == null)
                {
                    var container = Bible.Alarm.Container.Default;

                    UI.IocSetup.Initialize(container);
                    Bible.Alarm.Services.IocSetup.Initialize(container);
                    Bible.Alarm.Services.Droid.IocSetup.Initialize(container, isService);
                    Bible.Alarm.ViewModels.IocSetup.Initialize(container);
                    Bible.Alarm.Services.Droid.IocSetup.SetContext(context);

                    Context = context;
                    Container = container;    

                    return true;
                }

                return false;
            }
        }

    }
}