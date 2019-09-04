using Android.Content;
using JW.Alarm;

namespace Bible.Alarm.Droid
{
 
    public static class IocSetup
    {
        public static bool IsService { get; private set; }
        public static IContainer Container { get;  private set; }
        public static Context Context { get; private set; }
        public static void Initialize(Context context, bool isService)
        {
            var container = JW.Alarm.Container.Default;

            UI.IocSetup.Initialize(container);
            JW.Alarm.Services.IocSetup.Initialize(container);
            JW.Alarm.Services.Droid.IocSetup.Initialize(container, context, isService);
            JW.Alarm.ViewModels.IocSetup.Initialize(container);

            Container = container;
            Context = context;
            IsService = isService;
        }


    }
}