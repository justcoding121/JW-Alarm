using Android.Content;
using Bible.Alarm;

namespace Bible.Alarm.Droid
{

    public static class IocSetup
    {
        public static bool IsService { get; private set; }
        public static IContainer Container { get; private set; }
        public static Context Context { get; private set; }

        private static object @lock = new object();

        public static void Initialize(Context context, bool isService)
        {
            lock (@lock)
            {
                if(Container == null)
                {
                    var container = Bible.Alarm.Container.Default;

                    UI.IocSetup.Initialize(container);
                    Bible.Alarm.Services.IocSetup.Initialize(container);
                    Bible.Alarm.Services.Droid.IocSetup.Initialize(container, context, isService);
                    Bible.Alarm.ViewModels.IocSetup.Initialize(container);

                    Container = container;
                    Context = context;
                    IsService = isService;
                }
              
            }
        }

    }
}