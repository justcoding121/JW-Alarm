using Autofac;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;

namespace JW.Alarm.Services.Uwp.Helpers
{
    public class BootstrapHelper
    {
        private static readonly string alarmTaskName = "AlarmTask";
        private static readonly string schedulerTaskName = "SchedulerTask";

        public async static Task VerifyPermissions()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var hasInternetPermission = (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);

            if (!hasInternetPermission)
            {
                await displayNoInternetPermissionDialog();
            }
        }

        private static async Task displayNoInternetPermissionDialog()
        {
            var noInternetDialog = new ContentDialog
            {
                Title = "No internet connection",
                Content = "Alarms will only play your ringtone when not connected to internet.",
                CloseButtonText = "Ok"
            };

            var result = await noInternetDialog.ShowAsync();
        }

        public async static Task VerifyMediaLookUpService()
        {
            var service = IocSetup.Container.Resolve<MediaLookUpService>();
            await service.Verify();
        }

        public static void VerifyBackgroundTasks()
        {
            var allTasks = BackgroundTaskRegistration.AllTasks.Select(x => x.Value);

            var alarmTask = allTasks.FirstOrDefault(x => x.Name == alarmTaskName);

            if (alarmTask == null)
            {
                registerBackgroundTask(alarmTaskName, new ToastNotificationHistoryChangedTrigger());
            }

            var schedulerTask = allTasks.FirstOrDefault(x => x.Name == schedulerTaskName);

            if (schedulerTask == null)
            {
                registerBackgroundTask(schedulerTaskName, new TimeTrigger(15, false));
            }
        }


        public static BackgroundTaskRegistration registerBackgroundTask(string name, IBackgroundTrigger trigger)
        {
            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.SetTrigger(trigger);

            return builder.Register();
        }

        public static void unregisterBackgroundTasks(string name)
        {

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                }
            }

        }
    }
}
