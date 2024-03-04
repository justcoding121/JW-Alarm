using Android.Content;

namespace Bible.Alarm.Droid
{
    public static class ContainerExtension
    {
        public static Context AndroidContext(this IContainer container)
        {
            if (container.Context == null)
            {
                return null;
            }

            if (container.Context.ContainsKey("AndroidContext"))
            {
                return container.Context["AndroidContext"] as Context;
            }

            return null;
        }

        public static bool IsAndroidService(this IContainer container)
        {
            if (container.Context == null)
            {
                return false;
            }

            if (container.Context.ContainsKey("IsAndroidService"))
            {
                return (bool)container.Context["IsAndroidService"];
            }

            return false;
        }
    }
}