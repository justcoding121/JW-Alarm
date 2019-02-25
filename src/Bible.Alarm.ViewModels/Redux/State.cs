using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;

namespace JW.Alarm.ViewModels.Redux
{
    public class ApplicationState
    {
        public ObservableHashSet<ScheduleListItem> Schedules { get; set; }

        public ScheduleListItem ScheduleListItem { get; set; }
        public ScheduleViewModel ScheduleViewModel { get; set; }

        public MusicSelectionViewModel MusicSelectionViewModel { get; set; }
        public AlarmMusic CurrentMusic { get; set; }
    }
}
