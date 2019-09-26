using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Bible.Alarm.Common.DataStructures;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Mvvmicro;
using System.Reactive.Linq;
using System.ComponentModel;
using Bible.Alarm.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using Xamarin.Forms;
using Bible.Alarm.ViewModels.Redux;
using System.Reactive.Concurrency;
using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.Common.Mvvm;

namespace Bible.Alarm.ViewModels
{
    public class HomeViewModel : ViewModel, IDisposable
    {
        private ScheduleDbContext scheduleDbContext;
        private IToastService popUpService;
        private INavigationService navigationService;
        private IMediaCacheService mediaCacheService;
        private IAlarmService alarmService;

        private List<IDisposable> disposables = new List<IDisposable>();

        public HomeViewModel(ScheduleDbContext scheduleDbContext,
            IToastService popUpService, INavigationService navigationService,
            IMediaCacheService mediaCacheService,
            IAlarmService alarmService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.popUpService = popUpService;
            this.navigationService = navigationService;
            this.mediaCacheService = mediaCacheService;
            this.alarmService = alarmService;

            disposables.Add(scheduleDbContext);

            AddScheduleCommand = new Command(async () =>
            {
                ReduxContainer.Store.Dispatch(new ViewScheduleAction());
                var viewModel = IocSetup.Container.Resolve<ScheduleViewModel>();
                await navigationService.Navigate(viewModel);
            });

            ViewScheduleCommand = new Command<ScheduleListItem>(async x =>
            {
                x.Schedule.IsEnabled = x.IsEnabled;
                
                ReduxContainer.Store.Dispatch(new ViewScheduleAction()
                {
                    SelectedScheduleListItem = x
                });

                var viewModel = IocSetup.Container.Resolve<ScheduleViewModel>();
                await navigationService.Navigate(viewModel);
            });

            //set schedules from initial state.
            //this should fire only once (look at the where condition).
            var subscription = ReduxContainer.Store
               .Select(state => state.Schedules)
               .Where(x => x != null)
               .DistinctUntilChanged()
               .Take(1)
               .Subscribe(x =>
               {
                   Schedules = x;
                   listenIsEnabledChanges();
                   IsBusy = false;
               });
            disposables.Add(subscription);

            initialize();

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

        private void initialize()
        {
            Messenger<bool>.Subscribe(Messages.Initialized, async vm =>
            {
                var alarmSchedules = await scheduleDbContext.AlarmSchedules
                                    .AsNoTracking()
                                    .ToListAsync();

                var initialSchedules = new ObservableHashSet<ScheduleListItem>();
                foreach (var schedule in alarmSchedules)
                {
                    initialSchedules.Add(new ScheduleListItem(schedule));
                }

                ReduxContainer.Store.Dispatch(new InitializeAction() { ScheduleList = initialSchedules });
            });

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

                                     IsBusy = false;
                                 })
                                .Subscribe();

            disposables.Add(subscription);
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
        }
    }

    public class ScheduleListItem : ViewModel, IComparable
    {
        public AlarmSchedule Schedule;
        public ScheduleListItem(AlarmSchedule schedule)
        {
            Schedule = schedule;
            isEnabled = schedule.IsEnabled;
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
