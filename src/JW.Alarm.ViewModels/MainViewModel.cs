using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JW.Alarm.Services.Contracts;

namespace JW.Alarm.ViewModels
{

    public class MainViewModel : BindableBase
    {
        private IScheduleService scheduleService;
        private IThreadService threadService;

        public MainViewModel(IScheduleService scheduleService, IThreadService threadService)
        {
            this.scheduleService = scheduleService;
            this.threadService = threadService;
        }

        public ObservableCollection<ScheduleViewModel> Schedules { get; }
            = new ObservableCollection<ScheduleViewModel>();

        private ScheduleViewModel selectedSchedule;

        public ScheduleViewModel SelectedSchedule
        {
            get => selectedSchedule;
            set => Set(ref selectedSchedule, value);
        }

        private bool isLoading = false;

        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }


        public async Task GetScheduleListAsync()
        {
            await threadService.RunOnUIThread(()=> IsLoading = true);

            var schedules = await scheduleService.Schedules;
            if (schedules == null)
            {
                return;
            }

            await threadService.RunOnUIThread(() =>
            {
                Schedules.Clear();
                foreach (var schedule in schedules)
                {
                    Schedules.Add(new ScheduleViewModel(schedule.Value));
                }
                IsLoading = false;
            });
        }

    }
}
