using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mvvmicro;
using Redux;

namespace JW.Alarm.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        IScheduleRepository alarmDbContext;

        IAlarmService alarmService;
        IPopUpService popUpService;

        public ScheduleViewModel(AlarmSchedule model = null)
        {
            this.alarmDbContext = IocSetup.Container.Resolve<IScheduleRepository>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.alarmService = IocSetup.Container.Resolve<IAlarmService>();

            IsNewSchedule = model == null ? true : false;
            setModel(model ?? new AlarmSchedule());
        }

        public AlarmSchedule Model { get; private set; }

        private AlarmSchedule getModel()
        {
            Model.Id = scheduleId;

            Model.Name = Name;
            Model.IsEnabled = IsEnabled;
            Model.DaysOfWeek = DaysOfWeek;
            Model.Hour = Time.Hours;
            Model.Minute = Time.Minutes;
            Model.MusicEnabled = musicEnabled;

            return Model;
        }

        private void setModel(AlarmSchedule model)
        {
            this.Model = model;

            scheduleId = model.Id;
            name = model.Name;
            isEnabled = model.IsEnabled;
            daysOfWeek = model.DaysOfWeek;
            time = new TimeSpan(model.Hour, model.Minute, 0);
            musicEnabled = model.MusicEnabled;
        }

        private long scheduleId;

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

        public BibleSelectionViewModel GetBibleSelectionViewModel()
        {
            return new BibleSelectionViewModel(BibleReadingSchedule, new BibleReadingSchedule()
            {
                BookNumber = BibleReadingSchedule.BookNumber,
                ChapterNumber = BibleReadingSchedule.ChapterNumber,
                LanguageCode = BibleReadingSchedule.LanguageCode,
                PublicationCode = BibleReadingSchedule.PublicationCode
            });
        }

        public MusicSelectionViewModel GetMusicSelectionViewModel()
        {
            return new MusicSelectionViewModel(Music);
        }

        public AlarmMusic Music => Model.Music;
        public BibleReadingSchedule BibleReadingSchedule => Model.BibleReadingSchedule;

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

            this.RaiseProperty("DaysOfWeek");
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                await popUpService.ShowProgressRing();

                if (!await validate())
                {
                    return false;
                }

                var model = getModel();

                if (IsNewSchedule)
                {
                    await alarmDbContext.Add(model);
                    await alarmService.Create(model);
                    IsNewSchedule = false;
                }
                else
                {
                    await alarmDbContext.Update(model);
                    await alarmService.Update(model);
                }

                if (IsEnabled)
                {
                    await popUpService.ShowScheduledNotification(model);
                }

                return true;
            }
            finally
            {
                await popUpService.HideProgressRing();
            }
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
                await alarmDbContext.Remove(scheduleId);
                alarmService.Delete(scheduleId);
            }
        }
    }
}
