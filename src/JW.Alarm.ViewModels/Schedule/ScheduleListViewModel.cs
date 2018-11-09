using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Collections;

namespace JW.Alarm.ViewModels
{
    public class ScheduleListViewModel : ViewModelBase
    {
        private IScheduleDbContext scheduleService;
        private IThreadService threadService;
        private IPopUpService popUpService;

        public ScheduleListViewModel(IScheduleDbContext scheduleService,
            IThreadService threadService, IPopUpService popUpService)
        {
            this.scheduleService = scheduleService;
            this.threadService = threadService;
            this.popUpService = popUpService;

            Task.Run(() => InitializeScheduleListAsync());
        }

        private Dictionary<AlarmSchedule, ScheduleListItem> listMapping = new Dictionary<AlarmSchedule, ScheduleListItem>();
        public ObservableHashSet<ScheduleListItem> Schedules { get; } = new ObservableHashSet<ScheduleListItem>();

        private ScheduleViewModel selectedSchedule;

        public ScheduleViewModel SelectedSchedule
        {
            get => selectedSchedule;
            set => this.Set(ref selectedSchedule, value);
        }


        public async Task InitializeScheduleListAsync()
        {
            var scheduleObservable = Observable.FromEventPattern((EventHandler<NotifyCollectionChangedEventArgs> ev)
                               => new NotifyCollectionChangedEventHandler(ev),
                                     ev => Schedules.CollectionChanged += ev,
                                     ev => Schedules.CollectionChanged -= ev);

            var subscription = scheduleObservable
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
                                 .Merge()
                                 .Do(async x => await popUpService.ShowProgressRing())
                                 .Do(async y => await scheduleService.Update(y.Schedule))
                                 .Do(async y => { if (y.IsEnabled) await popUpService.ShowScheduledNotification(y.Schedule); })
                                 .Do(async x => await popUpService.HideProgressRing())
                                 .Subscribe();

            var alarmSchedules = await scheduleService.AlarmSchedules;

            alarmSchedules.CollectionChanged += async (s, e) =>
            {
                await threadService.RunOnUIThread(() =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            add(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            remove(e.OldItems);
                            break;
                    }
                });
            };

            await threadService.RunOnUIThread(() =>
            {
                foreach (var schedule in alarmSchedules)
                {
                    var listItem = new ScheduleListItem(schedule.Value);
                    listMapping.Add(schedule.Value, listItem);
                    Schedules.Add(listItem);
                }
            });

            await popUpService.HideProgressRing();
        }

        private void remove(IList oldItems)
        {
            foreach (var newItem in oldItems)
            {
                var removed = ((KeyValuePair<int, AlarmSchedule>)newItem).Value;
                Schedules.Remove(listMapping[removed]);
                listMapping.Remove(removed);
            }
        }

        private void add(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                var listItem = new ScheduleListItem(((KeyValuePair<int, AlarmSchedule>)newItem).Value);
                listMapping.Add(listItem.Schedule, listItem);
                Schedules.Add(listItem);
            }
        }

    }

    public class ScheduleListItem : ViewModelBase, IComparable
    {
        public AlarmSchedule Schedule;
        public ScheduleListItem(AlarmSchedule schedule = null)
        {
            Schedule = schedule;
            isEnabled = schedule.IsEnabled;
        }

        public int ScheduleId => Schedule.Id;

        public string Name => Schedule.Name;

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => this.Set(ref isEnabled, value);
        }

        public HashSet<DayOfWeek> DaysOfWeek => Schedule.DaysOfWeek;

        public string TimeText => Schedule.TimeText;

        public string Hour => Schedule.Hour.ToString("D2");

        public string Minute => Schedule.Minute.ToString("D2");

        public Meridien Meridien => Schedule.Meridien;

        public int CompareTo(object obj)
        {
            return ScheduleId.CompareTo((obj as ScheduleListItem).ScheduleId);
        }
    }
}
