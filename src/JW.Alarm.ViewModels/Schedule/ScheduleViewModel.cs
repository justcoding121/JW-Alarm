using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;

namespace JW.Alarm.ViewModels
{

    public class ScheduleViewModel : BindableBase
    {
        IScheduleService scheduleService;
        IPopUpService popUpService;

        public ScheduleViewModel(AlarmSchedule model = null)
        {
            this.scheduleService = IocSetup.Container.Resolve<IScheduleService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();

            isNewSchedule = model == null ? true : false;
            Model = model ?? new AlarmSchedule();

            EnableCommand = new RelayCommandAsync<object>(async (x) =>
            {
                IsEnabled = bool.Parse(x.ToString());
                await SaveAsync();

            });
        }

        private AlarmSchedule model;

        public AlarmSchedule Model
        {
            get => model;
            set
            {
                if (model != value)
                {
                    model = value;
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        public string Name
        {
            get => Model.Name;
            set
            {
                if (value != Model.Name)
                {
                    Model.Name = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set
            {
                if (value != Model.IsEnabled)
                {
                    Model.IsEnabled = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public HashSet<DayOfWeek> DaysOfWeek
        {
            get => Model.DaysOfWeek;
        }

        public TimeSpan Time
        {
            get => new TimeSpan(Model.Hour, Model.Minute, 0);
            set
            {
                if (value.Hours != Model.Hour || value.Minutes != Model.Minute)
                {
                    Model.Hour = value.Hours;
                    Model.Minute = value.Minutes;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
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

        public AlarmMusic Music
        {
            get => Model.Music;
            set
            {
                if (value != Model.Music)
                {
                    Model.Music = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public BibleAudio Bible
        {
            get => Model.Bible;
            set
            {
                if (value != Model.Bible)
                {
                    Model.Bible = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsModified { get; set; }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }

        private bool isNewSchedule;
        public bool IsNewSchedule => isNewSchedule;

        public bool IsExistingSchedule => !isNewSchedule;

        public ICommand EnableCommand { get; private set; }

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
            OnPropertyChanged("DaysOfWeek");
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

           
            if (isNewSchedule)
            {
                isNewSchedule = false;
                await scheduleService.Create(Model);
            }
            else
            {
                await scheduleService.Update(Model);
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
                await scheduleService.Delete(Model.Id);
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
            Model = (await scheduleService.Schedules)[Model.Id];
        }

    }
}
