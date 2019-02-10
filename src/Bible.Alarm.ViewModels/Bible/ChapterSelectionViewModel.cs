using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class ChapterSelectionViewModel : ViewModelBase, IDisposable
    {
        private MediaService mediaService;
        private IThreadService threadService;
        private IPopUpService popUpService;
        private IPreviewPlayService playService;
        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public ChapterSelectionViewModel(BibleReadingSchedule current, BibleReadingSchedule tentative)
        {
            this.current = current;
            this.tentative = tentative;

            this.current = current;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.playService = IocSetup.Container.Resolve<IPreviewPlayService>();

            initialize();
        }

        private void initialize()
        {
            Task.Run(() => InitializeAsync(tentative.LanguageCode, tentative.PublicationCode, tentative.BookNumber));
        }

        public ObservableHashSet<BibleChapterListViewItemModel> Chapters { get; set; } = new ObservableHashSet<BibleChapterListViewItemModel>();

        private BibleChapterListViewItemModel selectedChapter;
        public BibleChapterListViewItemModel SelectedChapter
        {
            get => selectedChapter;
            set
            {
                selectedChapter = value;
                RaiseProperty("SelectedChapter");
            }
        }

        public void SetChapter(BibleChapterListViewItemModel bibleChapterListViewItemModel)
        {
            SelectedChapter = bibleChapterListViewItemModel;

            tentative.ChapterNumber = bibleChapterListViewItemModel.Number;

            current.LanguageCode = tentative.LanguageCode;
            current.PublicationCode = tentative.PublicationCode;
            current.BookNumber = tentative.BookNumber;
            current.ChapterNumber = tentative.ChapterNumber;
        }


        private BibleChapterListViewItemModel currentlyPlaying;

        private async Task InitializeAsync(string languageCode, string publicationCode, int bookNumber)
        {
            var scheduleObservable = Observable.FromEventPattern((EventHandler<NotifyCollectionChangedEventArgs> ev)
                              => new NotifyCollectionChangedEventHandler(ev),
                                    ev => Chapters.CollectionChanged += ev,
                                    ev => Chapters.CollectionChanged -= ev);

            var subscription1 = scheduleObservable
                        .SelectMany(x =>
                        {
                            var newItems = x.EventArgs.NewItems?.Cast<BibleChapterListViewItemModel>();
                            if (newItems == null)
                            {
                                return Enumerable.Empty<IObservable<BibleChapterListViewItemModel>>();
                            }

                            return newItems.Select(added =>
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
                            });

                        })
                         .Merge()
                         .Do(async x => await popUpService.ShowProgressRing())
                         .Do(y =>
                         {
                             if (currentlyPlaying != null && currentlyPlaying != y)
                             {
                                 currentlyPlaying.Play = false;
                             }

                             currentlyPlaying = y;
                             playService.Play(y.Url);

                         })
                         .Do(async x => await popUpService.HideProgressRing())
                         .Subscribe();

            var subscription2 = scheduleObservable
                               .SelectMany(x =>
                               {
                                   var newItems = x.EventArgs.NewItems?.Cast<BibleChapterListViewItemModel>();
                                   if (newItems == null)
                                   {
                                       return Enumerable.Empty<IObservable<BibleChapterListViewItemModel>>();
                                   }

                                   return newItems.Select(added =>
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
                                   });

                               })
                                .Merge()
                                .Do(async x => await popUpService.ShowProgressRing())
                                .Do(y =>
                                {
                                    currentlyPlaying = null;
                                    playService.Stop();

                                })
                                .Do(async x => await popUpService.HideProgressRing())
                                .Subscribe();

            disposables.AddRange(new[] { subscription1, subscription2 });

            await populateChapters(languageCode, publicationCode, bookNumber);
        }

        private async Task populateChapters(string languageCode, string publicationCode, int bookNumber)
        {
            await popUpService.ShowProgressRing();

            var chapters = await mediaService.GetBibleChapters(languageCode, publicationCode, bookNumber);

            await threadService.RunOnUIThread(() =>
            {
                Chapters.Clear();
            });

            foreach (var chapter in chapters.Select(x => x.Value))
            {
                var chapterVM = new BibleChapterListViewItemModel(chapter);

                await threadService.RunOnUIThread(() =>
                {
                    Chapters.Add(chapterVM);
                });

                if (current.LanguageCode == tentative.LanguageCode
                    && current.PublicationCode == tentative.PublicationCode
                    && current.BookNumber == tentative.BookNumber
                    && tentative.ChapterNumber == chapter.Number)
                {
                    selectedChapter = chapterVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedChapter");
            });

            await popUpService.HideProgressRing();
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
            disposables.Clear();
        }
    }

    public class BibleChapterListViewItemModel : ViewModelBase, IComparable
    {
        private readonly BibleChapter chapter;
        public BibleChapterListViewItemModel(BibleChapter chapter)
        {
            this.chapter = chapter;
        }

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
