using Bible.Alarm.Common.DataStructures;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Contracts.Battery;
using Bible.Alarm.Models;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions;
using Microsoft.EntityFrameworkCore;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class HomeViewModel : ViewModel, IDisposable
    {
        private IContainer container;

        private ScheduleDbContext scheduleDbContext;
        private IToastService popUpService;
        private INavigationService navigationService;
        private IMediaCacheService mediaCacheService;
        private IAlarmService alarmService;
        private IBatteryOptimizationManager batteryOptimizationManager;
        private TaskScheduler uiTaskScheduler;

        private List<IDisposable> subscriptions = new List<IDisposable>();

        public Command BatteryOptimizationExcludeCommand { get; private set; }
        public Command BatteryOptimizationDismissCommand { get; private set; }

        public HomeViewModel(IContainer container, ScheduleDbContext scheduleDbContext,
            IToastService popUpService, INavigationService navigationService,
            IMediaCacheService mediaCacheService,
            IAlarmService alarmService,
            IBatteryOptimizationManager batteryOptimizationManager)
        {
            this.container = container;
            this.scheduleDbContext = scheduleDbContext;
            this.popUpService = popUpService;
            this.navigationService = navigationService;
            this.mediaCacheService = mediaCacheService;
            this.alarmService = alarmService;
            this.batteryOptimizationManager = batteryOptimizationManager;

            uiTaskScheduler = this.container.Resolve<TaskScheduler>();

            subscriptions.Add(scheduleDbContext);

            AddScheduleCommand = new Command(async () =>
            {
                ReduxContainer.Store.Dispatch(new ViewScheduleAction());
                var viewModel = this.container.Resolve<ScheduleViewModel>();
                await this.navigationService.Navigate(viewModel);
            });

            ViewScheduleCommand = new Command<ScheduleListItem>(async x =>
            {
                x.Schedule.IsEnabled = x.IsEnabled;

                ReduxContainer.Store.Dispatch(new ViewScheduleAction()
                {
                    SelectedScheduleListItem = x
                });

                var viewModel = this.container.Resolve<ScheduleViewModel>();
                await this.navigationService.Navigate(viewModel);
            });

            BatteryOptimizationExcludeCommand = new Command(async () =>
            {
                await markBatteryOptimizationModalAsShown();

                await navigationService.CloseModal();

                this.batteryOptimizationManager.ShowBatteryOptimizationExclusionSettingsPage();
            });

            BatteryOptimizationDismissCommand = new Command(async () =>
            {
                await markBatteryOptimizationModalAsShown();

                await navigationService.CloseModal();
            });


            //set schedules from initial state.
            //this should fire only once (look at the where condition).
            var subscription = ReduxContainer.Store
               .Select(state => state.Schedules)
               .Where(x => x != null)
               .DistinctUntilChanged()
               .Subscribe(async x =>
               {
                   Schedules = x;
                   listenIsEnabledChanges();
                   IsBusy = false;

                   await Task.Delay(10).ContinueWith(async (y) =>
                   {
                       if (Device.RuntimePlatform == Device.Android)
                       {
                           await showBatteryOptimizationExclusionPage();
                       }

                   }, uiTaskScheduler);
               });
            subscriptions.Add(subscription);

            initialize();

        }

        private async Task markBatteryOptimizationModalAsShown()
        {
            if (!await scheduleDbContext.GeneralSettings.AnyAsync(x => x.Key == "AndroidBatteryOptimizationExclusionPromptShown"))
            {
                await scheduleDbContext.GeneralSettings.AddAsync(new GeneralSettings()
                {
                    Key = "AndroidBatteryOptimizationExclusionPromptShown",
                    Value = "True"
                });

                await scheduleDbContext.SaveChangesAsync();
            }
        }

        private async Task showBatteryOptimizationExclusionPage()
        {
            if (!await scheduleDbContext.GeneralSettings.AnyAsync(x => x.Key == "AndroidBatteryOptimizationExclusionPromptShown"))
            {
                await navigationService.ShowModal("BatteryOptimizationExclusionModal", this);
            }
        }

        private async Task seedDefaultAlarm()
        {
            if (!await scheduleDbContext.AlarmSchedules.AnyAsync()
                && !await scheduleDbContext.GeneralSettings.AnyAsync(x => x.Key == "AlarmSeeded")
                //for existing apps before version 1.30
                && !await scheduleDbContext.GeneralSettings.AnyAsync(x => x.Key == "AndroidBatteryOptimizationExclusionPromptShown"))
            {
                var schedule = new AlarmSchedule()
                {
                    IsEnabled = false,
                    MusicEnabled = true,
                    DaysOfWeek = DaysOfWeek.All,
                    Name = "A sample alarm.",
                    Hour = 6,
                    Minute = 0,
                    Music = new AlarmMusic()
                    {
                        MusicType = MusicType.Vocals,
                        PublicationCode = "sjjc",
                        LanguageCode = "E",
                        TrackNumber = 16
                    },
                    BibleReadingSchedule = new BibleReadingSchedule()
                    {
                        BookNumber = 23,
                        ChapterNumber = 36,
                        LanguageCode = "E",
                        PublicationCode = "nwt"
                    }
                };

                await scheduleDbContext.AlarmSchedules.AddAsync(schedule);

                await scheduleDbContext.GeneralSettings.AddAsync(new GeneralSettings()
                {
                    Key = "AlarmSeeded",
                    Value = "True"
                });

                await scheduleDbContext.SaveChangesAsync();

                await alarmService.Create(schedule);
            }
        }

        private ObservableHashSet<ScheduleListItem> schedules;
        public ObservableHashSet<ScheduleListItem> Schedules
        {
            get => schedules;
            set => this.Set(ref schedules, value);
        }

        private bool isBusy = true;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                this.Set(ref isBusy, value);
                Loaded = !isBusy;
            }
        }

        private bool loaded = false;
        public bool Loaded
        {
            get => loaded;
            set => this.Set(ref loaded, value);
        }

        public ICommand AddScheduleCommand { get; set; }
        public ICommand ViewScheduleCommand { get; set; }

        private ScheduleViewModel selectedSchedule;

        public ScheduleViewModel SelectedSchedule
        {
            get => selectedSchedule;
            set => this.Set(ref selectedSchedule, value);
        }

        private bool initialized = false;
        private SemaphoreSlim @lock = new SemaphoreSlim(1);
        private void initialize()
        {
            Messenger<bool>.Subscribe(Messages.Initialized, async vm =>
            {
                await @lock.WaitAsync();

                try
                {
                    if (!initialized)
                    {
                        await seedDefaultAlarm();

                        var alarmSchedules = await scheduleDbContext.AlarmSchedules
                                            .AsNoTracking()
                                            .ToListAsync();

                        var initialSchedules = new ObservableHashSet<ScheduleListItem>();
                        foreach (var schedule in alarmSchedules)
                        {
                            initialSchedules.Add(new ScheduleListItem(container, schedule));
                        }

                        ReduxContainer.Store.Dispatch(new InitializeAction() { ScheduleList = initialSchedules });

                        initialized = true;
                    }
                }
                finally
                {
                    @lock.Release();
                }

            }, true);
        }

        private void listenIsEnabledChanges()
        {
            var scheduleListChangedObservable = Observable.FromEventPattern((EventHandler<NotifyCollectionChangedEventArgs> ev)
                            => new NotifyCollectionChangedEventHandler(ev),
                                  ev => Schedules.CollectionChanged += ev,
                                  ev => Schedules.CollectionChanged -= ev);

            //for schedules currently shown on screen.
            var isEnabledObservable = Schedules.Select(item =>
            {
                var removedObservable = scheduleListChangedObservable.Any(z =>
                {
                    var oldItem = z.EventArgs.OldItems?.Cast<ScheduleListItem>();
                    return oldItem != null && oldItem.Any(removed => item == removed);
                });

                //observe until the schedule is removed from the list.
                return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, ScheduleListItem>>(
                               onNextHandler => (object sender, PropertyChangedEventArgs e)
                                             => onNextHandler(new KeyValuePair<string, ScheduleListItem>(e.PropertyName, (ScheduleListItem)sender)),
                                               handler => item.PropertyChanged += handler,
                                               handler => item.PropertyChanged -= handler)
                                               .TakeUntil(removedObservable)
                                               .Where(kv => kv.Key == "IsEnabled")
                                               .Select(y => y.Value);
            }).Merge();

            //observe for all future schedules. 
            var isEnableObservableForNewSchedules = scheduleListChangedObservable
                                .SelectMany(x =>
                                {
                                    var newItems = x.EventArgs.NewItems?.Cast<ScheduleListItem>();
                                    if (newItems == null)
                                    {
                                        return Enumerable.Empty<IObservable<ScheduleListItem>>();
                                    }

                                    //observe until the schedule is removed from the list.
                                    return newItems.Select(added =>
                                    {
                                        var removedObservable = scheduleListChangedObservable.Any(z =>
                                        {
                                            var oldItem = z.EventArgs.OldItems?.Cast<ScheduleListItem>();
                                            return oldItem != null && oldItem.Any(removed => added == removed);
                                        });

                                        return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, ScheduleListItem>>(
                                                       onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                                     => onNextHandler(new KeyValuePair<string, ScheduleListItem>(e.PropertyName, (ScheduleListItem)sender)),
                                                                       handler => added.PropertyChanged += handler,
                                                                       handler => added.PropertyChanged -= handler)
                                                                       .TakeUntil(removedObservable)
                                                                       .Where(kv => kv.Key == "IsEnabled")
                                                                       .Select(y => y.Value);
                                    });
                                })
                                 .Merge();

            //now the actual job (show the scheduled notification).
            var subscription = Observable.Merge(isEnabledObservable, isEnableObservableForNewSchedules)
                                 .ObserveOn(Scheduler.CurrentThread)
                                 .Do(async y =>
                                 {
                                     IsBusy = true;

                                     await Task.Run(async () =>
                                     {
                                         var existing = await scheduleDbContext.AlarmSchedules.FirstAsync(x => x.Id == y.ScheduleId);
                                         existing.IsEnabled = y.IsEnabled;
                                         await scheduleDbContext.SaveChangesAsync();

                                         alarmService.Update(existing);
                                     });

                                     if (y.IsEnabled)
                                     {
                                         await popUpService.ShowScheduledNotification(y.Schedule);
                                     }

                                     y.RaisePropertiesChangedEvent();

                                     IsBusy = false;
                                 })
                                .Subscribe();

            subscriptions.Add(subscription);
        }

        public void Dispose()
        {
            subscriptions.ForEach(x => x.Dispose());

            this.scheduleDbContext.Dispose();
            this.popUpService.Dispose();
            this.mediaCacheService.Dispose();
            this.alarmService.Dispose();
            this.batteryOptimizationManager.Dispose();
            @lock.Dispose();
        }
    }

    public class ScheduleListItem : ViewModel, IComparable
    {
        private IContainer container;

        public AlarmSchedule Schedule;
        public ScheduleListItem(IContainer container, AlarmSchedule schedule)
        {
            this.container = container;

            Schedule = schedule;
            isEnabled = schedule.IsEnabled;

            PlayCommand = new Command(async () =>
            {
                if (Schedule.Id > 0)
                {
                    var toastService = container.Resolve<IToastService>();
                    await toastService.ShowMessage("Your music and or Bible audio will start in few seconds.", 5);

                    toastService.Dispose();

                    var notificationService = container.Resolve<INotificationService>();
                    notificationService.ShowNotification(Schedule.Id);

                    notificationService.Dispose();

                }

            });
        }

        public long ScheduleId => Schedule.Id;

        public string Name => Schedule.Name;

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => this.Set(ref isEnabled, value);
        }

        public DaysOfWeek DaysOfWeek => Schedule.DaysOfWeek;

        public string TimeText => Schedule.TimeText;

        public string Hour => Schedule.MeridienHour.ToString("D2");

        public string Minute => Schedule.Minute.ToString("D2");

        public Meridien Meridien => Schedule.Meridien;

        public ScheduleListItem This => this;

        public ICommand PlayCommand { get; private set; }

        public void RaisePropertiesChangedEvent()
        {
            RaiseProperties(GetType()
                .GetProperties()
                .Where(x => x.Name != "IsEnabled")
                .Select(x => x.Name).ToArray());
        }

        public int CompareTo(object obj)
        {
            return ScheduleId.CompareTo((obj as ScheduleListItem).ScheduleId);
        }
    }
}
