using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mvvmicro;
using Redux;
using JW.Alarm.Services;
using System.Windows.Input;
using Xamarin.Forms;
using Bible.Alarm.Services.Contracts;

namespace JW.Alarm.ViewModels
{
    public class ScheduleViewModel : ViewModel
    {
        ScheduleDbContext scheduleDbContext;
        IAlarmService alarmService;
        IPopUpService popUpService;
        INavigationService navigationService;
        private IThreadService threadService;

        public ScheduleViewModel(AlarmSchedule model = null)
        {
            this.scheduleDbContext = IocSetup.Container.Resolve<ScheduleDbContext>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.alarmService = IocSetup.Container.Resolve<IAlarmService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            IsNewSchedule = model == null ? true : false;
            setModel(model ?? new AlarmSchedule());

            CancelCommand = new Command(async () =>
            {
                await navigationService.GoBack();
            });

            SaveCommand = new Command(async () =>
            {
                if (await saveAsync())
                {
                    await navigationService.GoBack();
                }
            });

            DeleteCommand = new Command(async () =>
            {
                await deleteAsync();
                await navigationService.GoBack();
            });

            ToggleDayCommand = new Command<DaysOfWeek>(x =>
            {
                toggle(x);
            });

            SelectMusicCommand = new Command(async() =>
            {
                await navigationService.Navigate(getMusicSelectionViewModel());
            });

            SelectBibleCommand = new Command(async () =>
            {
                await navigationService.Navigate(getBibleSelectionViewModel());
            });
        }


        public ICommand CancelCommand { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ICommand SelectMusicCommand { get; set; }
        public ICommand SelectBibleCommand { get; set; }

        public ICommand ToggleDayCommand { get; set; }

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
            time = new TimeSpan(model.Hour, model.Minute, model.Second);
            musicEnabled = model.MusicEnabled;
        }

        private int scheduleId;

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
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

        private DaysOfWeek daysOfWeek;
        public DaysOfWeek DaysOfWeek
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

        private BibleSelectionViewModel getBibleSelectionViewModel()
        {
            return new BibleSelectionViewModel(BibleReadingSchedule, new BibleReadingSchedule()
            {
                BookNumber = BibleReadingSchedule.BookNumber,
                ChapterNumber = BibleReadingSchedule.ChapterNumber,
                LanguageCode = BibleReadingSchedule.LanguageCode,
                PublicationCode = BibleReadingSchedule.PublicationCode
            });
        }

        private MusicSelectionViewModel getMusicSelectionViewModel()
        {
            return new MusicSelectionViewModel(Music);
        }

        public AlarmMusic Music => Model.Music;
        public BibleReadingSchedule BibleReadingSchedule => Model.BibleReadingSchedule;

        public bool IsNewSchedule { get; private set; }
        public bool IsExistingSchedule => !IsNewSchedule;

        private void toggle(DaysOfWeek day)
        {
            if ((DaysOfWeek & day) == day)
            {
                DaysOfWeek = DaysOfWeek & ~day;
            }
            else
            {
                DaysOfWeek = DaysOfWeek | day;
            }

            this.RaiseProperty("DaysOfWeek");
        }

        private async Task<bool> saveAsync()
        {
            try
            {
                await threadService.RunOnUIThread(() =>
                {
                    IsBusy = true;
                });

                if (!await validate())
                {
                    return false;
                }

                var model = getModel();

                if (IsNewSchedule)
                {
                    await scheduleDbContext.AddAsync(model);
                    await scheduleDbContext.SaveChangesAsync();
                    await alarmService.Create(model);
                    IsNewSchedule = false;
                }
                else
                {
                    scheduleDbContext.AlarmSchedules.Attach(model);
                    await scheduleDbContext.SaveChangesAsync();
                    alarmService.Update(model);
                }

                if (IsEnabled)
                {
                    await popUpService.ShowScheduledNotification(model);
                }

                return true;
            }
            finally
            {
                await threadService.RunOnUIThread(() =>
                {
                    IsBusy = false;
                });
            }
        }

        private async Task<bool> validate()
        {
            if (DaysOfWeek == 0)
            {
                await popUpService.ShowMessage("Please select day(s) of Week.");
                return false;
            }

            return true;
        }

        private async Task deleteAsync()
        {
            if (scheduleId >= 0)
            {
                var model = getModel();
                scheduleDbContext.AlarmSchedules.Attach(model);
                scheduleDbContext.AlarmSchedules.Remove(model);
                await scheduleDbContext.SaveChangesAsync();
                alarmService.Delete(scheduleId);
            }
        }
    }
}
