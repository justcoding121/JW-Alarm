using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mvvmicro;

namespace JW.Alarm.ViewModels
{

    public class ScheduleViewModel : ViewModelBase
    {
        IAlarmScheduleService alarmScheduleService;
        IBibleReadingScheduleService bibleReadingScheduleService;

        IPopUpService popUpService;

        //TODO get rid of this when reactive extensions are used later
        ScheduleListViewModel mainViewModel;

        public ScheduleViewModel(AlarmSchedule model = null)
        {
            this.alarmScheduleService = IocSetup.Container.Resolve<IAlarmScheduleService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.bibleReadingScheduleService = IocSetup.Container.Resolve<IBibleReadingScheduleService>();
            this.mainViewModel = IocSetup.Container.Resolve<ScheduleListViewModel>();

            IsNewSchedule = model == null ? true : false;
            Model = model ?? new AlarmSchedule();
            //{
            //    Music = ,
            //    BibleReadingScheduleId = 
            //};

            EnableCommand = new AsyncRelayCommand(async (x) =>
            {
                IsEnabled = bool.Parse(x.ToString());
                await SaveAsync();
            });
        }

        private AlarmSchedule model;

        public AlarmSchedule Model
        {
            get => model;
            set => this.Set(ref model, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.Set(ref name, value);
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => this.Set(ref isEnabled, value);
        }

        public HashSet<DayOfWeek> DaysOfWeek
        {
            get => Model.DaysOfWeek;
        }

        private TimeSpan time;
        public TimeSpan Time
        {
            get => new TimeSpan(Model.Hour, Model.Minute, 0);
            set => this.Set(ref time, value);
          
        }

        public string Hour
        {
            get => (Model.Hour % 12).ToString("D2");
        }

        public string Minute
        {
            get => Model.Minute.ToString("D2");
        }

        public Meridien Meridien
        {
            get => Model.Meridien;
        }

        private AlarmMusic music;
        public AlarmMusic Music
        {
            get => Model.Music;
            set => this.Set(ref music, value);
        }

        private int bibleReadingScheduleId;
        public int BibleReadingScheduleId
        {
            get => Model.BibleReadingScheduleId;
            set => this.Set(ref bibleReadingScheduleId, value);
        }


        public bool IsModified { get; set; }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => this.Set(ref isLoading, value);
        }

        public bool IsNewSchedule { get; private set; }
        public bool IsExistingSchedule => !IsNewSchedule;

        public AsyncRelayCommand EnableCommand { get; private set; }

        public void Toggle(DayOfWeek day)
        {
            if (DaysOfWeek.Contains(day))
            {
                DaysOfWeek.Remove(day);
            }
            else
            {
                DaysOfWeek.Add(day);
            }
            IsModified = true;

            this.RaiseProperty("DaysOfWeek");
        }

        public async Task CancelAsync()
        {
            if (!IsNewSchedule)
            {
                await RevertChangesAsync();
            }
        }

        public async Task<bool> SaveAsync()
        {
            if(IsExistingSchedule && !IsModified)
            {
                return true;
            }

            if (!await validate())
            {
                return false;
            }

           
            if (IsNewSchedule)
            {
                IsNewSchedule = false;
                await alarmScheduleService.Create(Model);
                mainViewModel.Schedules.Add(this);
            }
            else
            {
                await alarmScheduleService.Update(Model);
            }

            IsModified = false;

            var nextFire = Model.NextFireDate();
            var timeSpan = nextFire - DateTimeOffset.Now;

            if (IsEnabled)
            {
                await popUpService.ShowMessage($"Alarm set for {timeSpan.Hours} hours and {timeSpan.Minutes} minutes from now.");
            }

            
            return true;
        }

        private async Task<bool> validate()
        {
            if (DaysOfWeek.Count == 0)
            {
                await popUpService.ShowMessage("Please select day(s) of Week.");
                return false;
            }

            return true;
        }

        public async Task DeleteAsync()
        {
            if (Model.Id >= 0)
            {
                await alarmScheduleService.Delete(Model.Id);
                mainViewModel.Schedules.Remove(this);
            }
        }

        public async Task RevertChangesAsync()
        {
            if (IsModified)
            {
                await RefreshScheduleAsync();
                IsModified = false;
            }
        }

        public async Task RefreshScheduleAsync()
        {
            Model = (await alarmScheduleService.AlarmSchedules)[Model.Id];
        }

    }
}
