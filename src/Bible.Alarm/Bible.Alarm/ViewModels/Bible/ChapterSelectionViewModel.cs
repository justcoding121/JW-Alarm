using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.ViewModels.Redux.Actions.Bible;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace JW.Alarm.ViewModels
{
    public class ChapterSelectionViewModel : ViewModel, IDisposable
    {
        private MediaService mediaService;
        private IToastService popUpService;
        private IPreviewPlayService playService;
        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;
        private INavigationService navigationService;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public ChapterSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IToastService>();
            this.playService = IocSetup.Container.Resolve<IPreviewPlayService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            BackCommand = new Command(async () =>
            {
                IsBusy = true;
                playService.Stop();
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
                IsBusy = false;
            });

            SetChapterCommand = new Command<BibleChapterListViewItemModel>(x =>
            {
                IsBusy = true;

                if (SelectedChapter != null)
                {
                    SelectedChapter.IsSelected = false;
                }

                SelectedChapter = x;
                SelectedChapter.IsSelected = true;

                tentative.ChapterNumber = x.Number;

                ReduxContainer.Store.Dispatch(new ChapterSelectedAction()
                {
                    CurrentBibleReadingSchedule = new BibleReadingSchedule()
                    {
                        LanguageCode = tentative.LanguageCode,
                        PublicationCode = tentative.PublicationCode,
                        BookNumber = tentative.BookNumber,
                        ChapterNumber = x.Number
                    }
                });
                IsBusy = false;
            });

            //set schedules from initial state.
            //this should fire only once 
            var subscription1 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                   .Select(state => new { state.CurrentBibleReadingSchedule, state.TentativeBibleReadingSchedule })
                   .Where(x => x.CurrentBibleReadingSchedule != null && x.TentativeBibleReadingSchedule != null)
                   .DistinctUntilChanged()
                   .Take(1)
                   .Subscribe(async x =>
                   {
                       current = x.CurrentBibleReadingSchedule;
                       tentative = x.TentativeBibleReadingSchedule;
                       await initialize(tentative.LanguageCode, tentative.PublicationCode, tentative.BookNumber);
                       IsBusy = false;
                   });


            disposables.Add(subscription1);
        }

        public ICommand BackCommand { get; set; }
        public ICommand SetChapterCommand { get; set; }

        public BibleChapterListViewItemModel SelectedChapter { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        private ObservableCollection<BibleChapterListViewItemModel> chapters;
        public ObservableCollection<BibleChapterListViewItemModel> Chapters
        {
            get => chapters;
            set => this.Set(ref chapters, value);
        }

        private BibleChapterListViewItemModel currentlyPlaying;

        private async Task initialize(string languageCode, string publicationCode, int bookNumber)
        {
            await populateChapters(languageCode, publicationCode, bookNumber);


            var subscription1 = Chapters.Select(added =>
                            {
                                return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, BibleChapterListViewItemModel>>(
                                   onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                 => onNextHandler(new KeyValuePair<string, BibleChapterListViewItemModel>(e.PropertyName,
                                                                            (BibleChapterListViewItemModel)sender)),
                                                   handler => added.PropertyChanged += handler,
                                                   handler => added.PropertyChanged -= handler)
                                                   .Where(kv => kv.Key == "Play")
                                                   .Select(y => y.Value)
                                                   .Where(y => y.Play);
                            })
                             .Merge()
                             .Do(x => IsBusy = true)
                             .Do(y =>
                             {
                                 if (currentlyPlaying != null && currentlyPlaying != y)
                                 {
                                     currentlyPlaying.Play = false;
                                 }

                                 currentlyPlaying = y;
                                 playService.Play(y.Url);

                             })
                             .Do(x => IsBusy = false)
                             .Subscribe();

            var subscription2 = Chapters.Select(added =>
                                {
                                    return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, BibleChapterListViewItemModel>>(
                                                   onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                                 => onNextHandler(new KeyValuePair<string, BibleChapterListViewItemModel>(e.PropertyName,
                                                                                            (BibleChapterListViewItemModel)sender)),
                                                                   handler => added.PropertyChanged += handler,
                                                                   handler => added.PropertyChanged -= handler)
                                                                   .Where(kv => kv.Key == "Play")
                                                                   .Select(y => y.Value)
                                                                   .Where(y => !y.Play);
                                })
                                .Merge()
                               .Do(x => IsBusy = true)
                                .Do(y =>
                                {
                                    currentlyPlaying = null;
                                    playService.Stop();

                                })
                               .Do(x => IsBusy = false)
                                .Subscribe();

            var subscription3 = Observable.FromEvent(ev => playService.OnStopped += ev,
                                                    ev => playService.OnStopped -= ev)
                                .Do(y =>
                                {
                                    if (currentlyPlaying != null)
                                    {
                                        currentlyPlaying.Play = false;
                                    }
                                })
                                .Subscribe();

            disposables.AddRange(new[] { subscription1, subscription2, subscription3 });


        }

        private async Task populateChapters(string languageCode, string publicationCode, int bookNumber)
        {
            var chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, bookNumber);
            var chapterVMs = new ObservableCollection<BibleChapterListViewItemModel>();

            foreach (var chapter in chapters.Select(x => x.Value))
            {
                var chapterVM = new BibleChapterListViewItemModel(chapter);

                chapterVMs.Add(chapterVM);

                if (current.LanguageCode == tentative.LanguageCode
                    && current.PublicationCode == tentative.PublicationCode
                    && current.BookNumber == tentative.BookNumber
                    && current.ChapterNumber == chapter.Number)
                {
                    chapterVM.IsSelected = true;
                    SelectedChapter = chapterVM;
                }
            }

            Chapters = chapterVMs;
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
            disposables.Clear();
        }
    }

    public class BibleChapterListViewItemModel : ViewModel, IComparable
    {
        private readonly BibleChapter chapter;
        public BibleChapterListViewItemModel(BibleChapter chapter)
        {
            this.chapter = chapter;
            TogglePlayCommand = new Command(() => Play = !Play);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public ICommand TogglePlayCommand { get; set; }

        public int Number => chapter.Number;

        public string Title => chapter.Title;
        public string Url => chapter.Source.Url;
        public TimeSpan Duration => chapter.Source.Duration;

        private bool play;
        public bool Play
        {
            get => play;
            set => this.Set(ref play, value);
        }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as BibleChapterListViewItemModel).Number);
        }
    }
}
