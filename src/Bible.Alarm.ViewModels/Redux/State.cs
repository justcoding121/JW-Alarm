using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.ViewModels.Redux
{
    public class ApplicationState
    {
        public ObservableHashSet<ScheduleListItem> Schedules { get; set; }

        public ScheduleListItem ScheduleListItem { get; set; }
        public ScheduleViewModel ScheduleViewModel { get; set; }
    }
}
