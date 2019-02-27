using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System.Reactive.Linq;
using System.ComponentModel;
using JW.Alarm.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using Xamarin.Forms;
using Bible.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using System.Reactive.Concurrency;
using Bible.Alarm.ViewModels.Redux.Actions;

namespace JW.Alarm.ViewModels
{
    public class HomeViewModel : ViewModel, IDisposable
    {
        private ScheduleDbContext scheduleDbContext;
        private IThreadService threadService;
        private IPopUpService popUpService;
        private INavigationService navigationService;

        private List<IDisposable> disposables = new List<IDisposable>();

        public HomeViewModel(ScheduleDbContext scheduleDbContext,
            IThreadService threadService, IPopUpService popUpService, INavigationService navigationService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.threadService = threadService;
            this.popUpService = popUpService;
            this.navigationService = navigationService;

            disposables.Add(scheduleDbContext);

            AddScheduleCommand = new Command(async () =>
            {
                var viewModel = IocSetup.Container.Resolve<ScheduleViewModel>();
                await navigationService.Navigate(viewModel);
                ReduxContainer.Store.Dispatch(new ViewScheduleAction() { ScheduleViewModel = viewModel });
            });

            ViewScheduleCommand = new Command<ScheduleListItem>(async x =>
            {
                var viewModel = IocSetup.Container.Resolve<ScheduleViewModel>();
                await navigationService.Navigate(viewModel);
                ReduxContainer.Store.Dispatch(new ViewScheduleAction()
                {
                    ScheduleViewModel = viewModel,
                    SelectedScheduleListItem = x
                });
            });

            //set schedules from initial state.
            //this should fire only once (look at the where condition).
            var subscription = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
               .Select(state => state.Schedules)
               .Where(x => x != null)
               .DistinctUntilChanged()
               .Take(1)
               .Subscribe(x =>
               {
                   Schedules = x;
                   IsBusy = false;
                   listenIsEnabledChanges();
               });
            disposables.Add(subscription);

            Task.Run(() => initializeSchedulesAsync());
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
            set => this.Set(ref isBusy, value);
        }

        public ICommand AddScheduleCommand { get; set; }
        public ICommand ViewScheduleCommand { get; set; }

        private ScheduleViewModel selectedSchedule;

        public ScheduleViewModel SelectedSchedule
        {
            get => selectedSchedule;
            set => this.Set(ref selectedSchedule, value);
        }

        private async Task initializeSchedulesAsync()
        {
            var alarmSchedules = await scheduleDbContext.AlarmSchedules.AsNoTracking().ToListAsync();

            var initialSchedules = new ObservableHashSet<ScheduleListItem>();
            foreach (var schedule in alarmSchedules)
            {
                initialSchedules.Add(new ScheduleListItem(schedule));
            }

            ReduxContainer.Store.Dispatch(new InitializeAction() { ScheduleList = initialSchedules });
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
                                     var existing = await scheduleDbContext.AlarmSchedules.FirstAsync(x => x.Id == y.ScheduleId);
                                     existing.IsEnabled = y.IsEnabled;
                                     await scheduleDbContext.SaveChangesAsync();
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
        public ScheduleListItem(AlarmSchedule schedule = null)
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
            RaiseProperties(GetType().GetProperties().Select(x => x.Name).ToArray());
        }

        public int CompareTo(object obj)
        {
            return ScheduleId.CompareTo((obj as ScheduleListItem).ScheduleId);
        }
    }
}
