using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Mvvmicro;

namespace JW.Alarm.ViewModels
{
    public class ScheduleListViewModel : ViewModelBase
    {
        private IAlarmScheduleService scheduleService;
        private IThreadService threadService;
        private IPopUpService popUpService;

        public AsyncRelayCommand EnableCommand { get; private set; }

        public ScheduleListViewModel(IAlarmScheduleService scheduleService, 
            IThreadService threadService, IPopUpService popUpService)
        {
            this.scheduleService = scheduleService;
            this.threadService = threadService;
            this.popUpService = popUpService;

            EnableCommand = new AsyncRelayCommand(async (parameter, cancelationToken) =>
            {
                var scheduleId = int.Parse(parameter.ToString());
                var schedule = await scheduleService.Read(scheduleId);
                schedule.IsEnabled = !schedule.IsEnabled;
                await scheduleService.Update(schedule);

                if (schedule.IsEnabled)
                {
                    var nextFire = schedule.NextFireDate();
                    var timeSpan = nextFire - DateTimeOffset.Now;
                    await popUpService.ShowMessage($"Alarm set for {timeSpan.Hours} hours and {timeSpan.Minutes} minutes from now.");
                }
            });

            Task.Run(() => GetScheduleListAsync());
        }

        private Dictionary<AlarmSchedule, ScheduleListItem> listMapping = new Dictionary<AlarmSchedule, ScheduleListItem>();
        public ObservableHashSet<ScheduleListItem> Schedules { get; } = new ObservableHashSet<ScheduleListItem>();

        private ScheduleViewModel selectedSchedule;

        public ScheduleViewModel SelectedSchedule
        {
            get => selectedSchedule;
            set => this.Set(ref selectedSchedule, value);
        }

        private bool isLoading = false;


        public bool IsLoading
        {
            get => isLoading;
            set => this.Set(ref isLoading, value);
        }


        public async Task GetScheduleListAsync()
        {
            await threadService.RunOnUIThread(() => IsLoading = true);

            var schedules = await scheduleService.AlarmSchedules;
            if (schedules == null)
            {
                return;
            }

            schedules.CollectionChanged += async (s, e) =>
            {
                await threadService.RunOnUIThread(() =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            var listItem = new ScheduleListItem(((KeyValuePair<int, AlarmSchedule>)newItem).Value);
                            listMapping.Add(listItem.Schedule, listItem);
                            Schedules.Add(listItem);
                        }
                    }

                    if(e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var newItem in e.OldItems)
                        {
                            var removed = ((KeyValuePair<int, AlarmSchedule>)newItem).Value;
                            Schedules.Remove(listMapping[removed]);
                            listMapping.Remove(removed);
                        }
                    }
                });
            };

            await threadService.RunOnUIThread(() =>
            {
                Schedules.Clear();
                foreach (var schedule in schedules)
                {
                    var listItem = new ScheduleListItem(schedule.Value);
                    listMapping.Add(schedule.Value, listItem);
                    Schedules.Add(listItem);
                }
                IsLoading = false;
            });
        }


    }

    public class ScheduleListItem : IComparable
    {
        public AlarmSchedule Schedule;
        public ScheduleListItem(AlarmSchedule schedule = null)
        {
            Schedule = schedule;
        }

        public int ScheduleId => Schedule.Id;

        public string Name => Schedule.Name;

        public bool IsEnabled => Schedule.IsEnabled;

        public HashSet<DayOfWeek> DaysOfWeek => Schedule.DaysOfWeek;

        public string TimeText => Schedule.TimeText;

        public string Hour => Schedule.Hour.ToString("D2");

        public string Minute => Schedule.Minute.ToString("D2");

        public Meridien Meridien => Schedule.Meridien;

        public int CompareTo(object obj)
        {
            return ScheduleId.CompareTo((obj as ScheduleListItem).ScheduleId);
        }
    }

}
