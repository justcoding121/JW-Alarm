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

        public ScheduleListViewModel(IAlarmScheduleService scheduleService, IThreadService threadService)
        {
            this.scheduleService = scheduleService;
            this.threadService = threadService;
            Task.Run(() => GetScheduleListAsync());
        }

        public ObservableHashSet<AlarmSchedule> Schedules { get; } = new ObservableHashSet<AlarmSchedule>();

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
                            Schedules.Add(((KeyValuePair<int, AlarmSchedule>)newItem).Value);
                        }
                    }

                    if(e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            Schedules.Remove(((KeyValuePair<int, AlarmSchedule>)newItem).Value);
                        }
                    }
                });
            };

            await threadService.RunOnUIThread(() =>
            {
                Schedules.Clear();
                foreach (var schedule in schedules)
                {
                    Schedules.Add(schedule.Value);
                }
                IsLoading = false;
            });
        }


    }
}
