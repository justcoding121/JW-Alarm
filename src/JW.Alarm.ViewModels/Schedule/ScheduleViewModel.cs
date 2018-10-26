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
            setModel(model ?? new AlarmSchedule());

        }

        private AlarmSchedule getModel()
        {
            var model = new AlarmSchedule();

            model.Id = scheduleId;

            model.Name = Name;
            model.IsEnabled = IsEnabled;
            model.DaysOfWeek = DaysOfWeek;
            model.Hour = Time.Hours;
            model.Minute = Time.Minutes;
            model.MusicEnabled = musicEnabled;
            model.BibleReadingEnabled = BibleReadingEnabled;

            return model;
        }

        private void setModel(AlarmSchedule model)
        {
            scheduleId = model.Id;

            name = model.Name;
            isEnabled = model.IsEnabled;
            daysOfWeek = model.DaysOfWeek;
            time = new TimeSpan(model.Hour, model.Minute, 0);
            musicEnabled = model.MusicEnabled;
            bibleReadingEnabled = model.BibleReadingEnabled;
        }

        private int scheduleId;

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

        private HashSet<DayOfWeek> daysOfWeek;
        public HashSet<DayOfWeek> DaysOfWeek
        {
            get => daysOfWeek;
            set => this.Set(ref daysOfWeek, value);
        }

        private TimeSpan time;
        public TimeSpan Time
        {
            get => time;
            set => this.Set(ref time, value);

        }

        public string Hour
        {
            get => (Time.Hours % 12).ToString("D2");
        }

        public string Minute
        {
            get => Time.Minutes.ToString("D2");
        }

        public Meridien Meridien
        {
            get => Time.Hours < 12 ? Meridien.AM : Meridien.PM;
        }

        private bool musicEnabled;
        public bool MusicEnabled
        {
            get => musicEnabled;
            set => this.Set(ref musicEnabled, value);
        }

        private bool bibleReadingEnabled;
        public bool BibleReadingEnabled
        {
            get => bibleReadingEnabled;
            set => this.Set(ref bibleReadingEnabled, value);
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

        public async Task<bool> SaveAsync()
        {
            if (IsExistingSchedule && !IsModified)
            {
                return true;
            }

            if (!await validate())
            {
                return false;
            }

            var model = getModel();

            if (IsNewSchedule)
            {
                IsNewSchedule = false;
                await alarmScheduleService.Create(model);
            }
            else
            {
                await alarmScheduleService.Update(model);
            }

            IsModified = false;


            if (IsEnabled)
            {
                var nextFire = model.NextFireDate();
                var timeSpan = nextFire - DateTimeOffset.Now;
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
            if (scheduleId >= 0)
            {
                await alarmScheduleService.Delete(scheduleId);
            }
        }

    }
}
