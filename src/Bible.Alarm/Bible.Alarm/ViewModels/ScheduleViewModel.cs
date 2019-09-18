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
using System.Linq;
using JW.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions;
using Microsoft.EntityFrameworkCore;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace JW.Alarm.ViewModels
{
    public class ScheduleViewModel : ViewModel, IDisposable
    {
        ScheduleDbContext scheduleDbContext;
        IAlarmService alarmService;
        IToastService popUpService;
        INavigationService navigationService;
        IPlaybackService playbackService;
        INotificationService notificationService;

        private List<IDisposable> disposables = new List<IDisposable>();

        public ScheduleViewModel()
        {
            this.scheduleDbContext = IocSetup.Container.Resolve<ScheduleDbContext>();
            this.popUpService = IocSetup.Container.Resolve<IToastService>();
            this.alarmService = IocSetup.Container.Resolve<IAlarmService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();
            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
            this.notificationService = IocSetup.Container.Resolve<INotificationService>();

            disposables.Add(scheduleDbContext);

            //set schedules from initial state.
            //this should fire only once (look at the where condition).
            var subscription = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
               .Select(state => state.CurrentScheduleListItem)
               .DistinctUntilChanged()
               .Take(1)
               .Subscribe(x =>
               {
                   scheduleListItem = x;
                   var model = x?.Schedule;

                   IsNewSchedule = model == null ? true : false;
                   setModel(model ?? new AlarmSchedule()
                   {
                       IsEnabled = true,
                       MusicEnabled = true,
                       DaysOfWeek = DaysOfWeek.All,
                       Music = new AlarmMusic()
                       {
                           MusicType = MusicType.Melodies,
                           PublicationCode = "iam",
                           LanguageCode = "E",
                           TrackNumber = 89
                       },

                       BibleReadingSchedule = new BibleReadingSchedule()
                       {
                           BookNumber = 23,
                           ChapterNumber = 1,
                           LanguageCode = "E",
                           PublicationCode = "nwt"
                       }
                   });

                   IsBusy = false;
               });
            disposables.Add(subscription);


            var subscription2 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
           .Select(state => state.CurrentMusic)
           .Where(x => x != null && x != Music)
           .Subscribe(x =>
           {
               Music = x;
               musicUpdated = true;
           });

            disposables.Add(subscription2);

            var subscription3 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
            .Select(state => state.CurrentBibleReadingSchedule)
            .Where(x => x != null && x != BibleReadingSchedule)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                BibleReadingSchedule = x;
                bibleReadingUpdated = true;
            });

            disposables.Add(subscription3);

            CancelCommand = new Command(async () =>
            {
                IsBusy = true;

                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));

                IsBusy = false;
            });

            SaveCommand = new Command(async () =>
            {
                IsBusy = true;

                await Task.Run(async () =>
                {
                    if (!IsNewSchedule)
                    {
                        this.playbackService.Dismiss();
                    }

                    if (await saveAsync())
                    {
                        await navigationService.GoBack();
                        ReduxContainer.Store.Dispatch(new BackAction(this));
                    }
                });

                IsBusy = false;
            });

            DeleteCommand = new Command(async () =>
            {
                IsBusy = true;

                await Task.Run(async () =>
                {

                    this.playbackService.Dismiss();
                    await deleteAsync();
                    await navigationService.GoBack();

                    ReduxContainer.Store.Dispatch(new BackAction(this));
                });

                IsBusy = false;
            });

            ToggleDayCommand = new Command<DaysOfWeek>(x =>
            {
                toggle(x);
            });

            SelectMusicCommand = new Command(async () =>
            {
                IsBusy = true;

                await Task.Run(async () =>
                {
                    var viewModel = IocSetup.Container.Resolve<MusicSelectionViewModel>();
                    await navigationService.Navigate(viewModel);

                    //get the latest music track
                    if (Music == null || (!IsNewSchedule && !musicUpdated))
                    {
                        Music = await scheduleDbContext.AlarmMusic
                        .AsNoTracking()
                        .FirstAsync(x => x.AlarmScheduleId == scheduleId);
                    }

                    ReduxContainer.Store.Dispatch(new MusicSelectionAction()
                    {
                        CurrentMusic = Music
                    });
                });

                IsBusy = false;
            });

            SelectBibleCommand = new Command(async () =>
            {
                IsBusy = true;

                await Task.Run(async () =>
                {
                    var viewModel = IocSetup.Container.Resolve<BibleSelectionViewModel>();
                    await navigationService.Navigate(viewModel);

                    //get the latest bible track
                    if (BibleReadingSchedule == null || (!IsNewSchedule && !bibleReadingUpdated))
                    {
                        BibleReadingSchedule = await scheduleDbContext.BibleReadingSchedules
                                                .AsNoTracking()
                                                .FirstAsync(x => x.AlarmScheduleId == scheduleId);
                    }

                    ReduxContainer.Store.Dispatch(new BibleSelectionAction()
                    {
                        CurrentBibleReadingSchedule = BibleReadingSchedule,
                        TentativeBibleReadingSchedule = new BibleReadingSchedule()
                        {
                            PublicationCode = BibleReadingSchedule.PublicationCode,
                            LanguageCode = BibleReadingSchedule.LanguageCode
                        }
                    });
                });

                IsBusy = false;
            });
        }

        private ScheduleListItem scheduleListItem;

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

        private bool musicUpdated { get; set; }
        public AlarmMusic Music { get => Model.Music; set => Model.Music = value; }

        private bool bibleReadingUpdated { get; set; }
        public BibleReadingSchedule BibleReadingSchedule { get => Model.BibleReadingSchedule; set => Model.BibleReadingSchedule = value; }

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

                ReduxContainer.Store.Dispatch(new AddScheduleAction() { ScheduleListItem = new ScheduleListItem(model) });
            }
            else
            {
                var existing = await scheduleDbContext.AlarmSchedules
                    .Include(x => x.Music)
                    .Include(x => x.BibleReadingSchedule)
                    .FirstAsync(x => x.Id == model.Id);

                existing.Hour = model.Hour;
                existing.Minute = model.Minute;
                existing.DaysOfWeek = model.DaysOfWeek;
                existing.IsEnabled = model.IsEnabled;

                if (model.Music != null && musicUpdated)
                {
                    existing.Music.Fixed = model.Music.Fixed;
                    existing.Music.LanguageCode = model.Music.LanguageCode;
                    existing.Music.MusicType = model.Music.MusicType;
                    existing.Music.PublicationCode = model.Music.PublicationCode;
                    existing.Music.TrackNumber = model.Music.TrackNumber;
                }

                if (model.BibleReadingSchedule != null && bibleReadingUpdated)
                {
                    existing.BibleReadingSchedule.BookNumber = model.BibleReadingSchedule.BookNumber;
                    existing.BibleReadingSchedule.ChapterNumber = model.BibleReadingSchedule.ChapterNumber;
                    existing.BibleReadingSchedule.LanguageCode = model.BibleReadingSchedule.LanguageCode;
                    existing.BibleReadingSchedule.PublicationCode = model.BibleReadingSchedule.PublicationCode;
                }

                existing.MusicEnabled = model.MusicEnabled;
                existing.Name = model.Name;
                existing.Second = model.Second;
                existing.SnoozeMinutes = model.SnoozeMinutes;

                await scheduleDbContext.SaveChangesAsync();

                alarmService.Update(model);

                scheduleListItem.RaisePropertiesChangedEvent();
                ReduxContainer.Store.Dispatch(new UpdateScheduleAction() { ScheduleListItem = scheduleListItem });
            }

            if (IsEnabled)
            {
                await popUpService.ShowScheduledNotification(model);
            }

            return true;

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
                var model = scheduleDbContext.AlarmSchedules.FirstOrDefault(x => x.Id == scheduleId);
                scheduleDbContext.AlarmSchedules.Remove(model);
                await scheduleDbContext.SaveChangesAsync();
                alarmService.Delete(scheduleId);

                ReduxContainer.Store.Dispatch(new RemoveScheduleAction() { ScheduleListItem = scheduleListItem });
            }
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
        }
    }
}
