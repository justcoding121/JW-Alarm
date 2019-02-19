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

        public HomeViewModel(ScheduleDbContext scheduleDbContext,
            IThreadService threadService, IPopUpService popUpService, INavigationService navigationService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.threadService = threadService;
            this.popUpService = popUpService;
            this.navigationService = navigationService;

            AddScheduleCommand = new Command(async () =>
            {
                var viewModel = new ScheduleViewModel();
                ReduxContainer.Store.Dispatch(new ViewScheduleAction() { ScheduleViewModel = viewModel });
                await navigationService.Navigate(viewModel);
            });

            ViewScheduleCommand = new Command<ScheduleListItem>(async x =>
            {
                var viewModel = new ScheduleViewModel(x);
                ReduxContainer.Store.Dispatch(new ViewScheduleAction() { ScheduleViewModel = viewModel });
                await navigationService.Navigate(viewModel);
            });

            ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
               .DistinctUntilChanged(state => state.Schedules)
               .Where(x => x.Schedules != null)
               .Subscribe(x =>
               {
                   Schedules = x.Schedules;
                   listenChange();
               });

            Task.Run(() => initializeSchedulesAsync());
        }

        private ObservableHashSet<ScheduleListItem> schedules;
        public ObservableHashSet<ScheduleListItem> Schedules
        {
            get => schedules;
            set => this.Set(ref schedules, value);
        }

        private bool isBusy;
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
            var alarmSchedules = await scheduleDbContext.AlarmSchedules.ToListAsync();

            var initialSchedules = new ObservableHashSet<ScheduleListItem>();
            foreach (var schedule in alarmSchedules)
            {
                initialSchedules.Add(new ScheduleListItem(schedule));
            }
            await threadService.RunOnUIThread(() =>
            {
                ReduxContainer.Store.Dispatch(new InitializeAction() { ScheduleList = initialSchedules });
                IsBusy = false;
            });
        }

        private void listenChange()
        {
            var scheduleObservable = Observable.FromEventPattern((EventHandler<NotifyCollectionChangedEventArgs> ev)
                            => new NotifyCollectionChangedEventHandler(ev),
                                  ev => Schedules.CollectionChanged += ev,
                                  ev => Schedules.CollectionChanged -= ev);

            var existingChangedObservable = Schedules.Select(item =>
            {
                var removedObservable = scheduleObservable.Any(z =>
                {
                    var oldItem = z.EventArgs.OldItems?.Cast<ScheduleListItem>();
                    return oldItem != null && oldItem.Any(removed => item == removed);
                });

                return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, ScheduleListItem>>(
                               onNextHandler => (object sender, PropertyChangedEventArgs e)
                                             => onNextHandler(new KeyValuePair<string, ScheduleListItem>(e.PropertyName, (ScheduleListItem)sender)),
                                               handler => item.PropertyChanged += handler,
                                               handler => item.PropertyChanged -= handler)
                                               .TakeUntil(removedObservable)
                                               .Where(kv => kv.Key == "IsEnabled")
                                               .Select(y => y.Value);
            }).Merge();

            var newChangedObservable = scheduleObservable
                                .SelectMany(x =>
                                {
                                    var newItems = x.EventArgs.NewItems?.Cast<ScheduleListItem>();
                                    if (newItems == null)
                                    {
                                        return Enumerable.Empty<IObservable<ScheduleListItem>>();
                                    }

                                    return newItems.Select(added =>
                                    {
                                        var removedObservable = scheduleObservable.Any(z =>
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

            var subcription = Observable.Merge(existingChangedObservable, newChangedObservable)
                                 .Do(async x => await threadService.RunOnUIThread(() =>
                                 {
                                     IsBusy = true;
                                 }))
                                 .Do(async y =>
                                 {
                                     scheduleDbContext.AlarmSchedules.Attach(y.Schedule);
                                     await scheduleDbContext.SaveChangesAsync();
                                 })
                                 .Do(async y => { if (y.IsEnabled) await popUpService.ShowScheduledNotification(y.Schedule); })
                                 .Do(async x => await threadService.RunOnUIThread(() =>
                                 {
                                     IsBusy = false;
                                 }))
                                 .Subscribe();
        }

        public void Dispose()
        {
            scheduleDbContext.Dispose();
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

        public string Hour => Schedule.Hour.ToString("D2");

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
