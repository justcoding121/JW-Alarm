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
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace JW.Alarm.ViewModels
{
    public class BookSelectionViewModel : ViewModel, IDisposable
    {
        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;

        private MediaService mediaService;
        private IToastService popUpService;
        private INavigationService navigationService;

        public ICommand BackCommand { get; set; }
        public ICommand ChapterSelectionCommand { get; set; }

        private List<IDisposable> disposables = new List<IDisposable>();

        public BookSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IToastService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            BackCommand = new Command(async () =>
            {
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
            });

            ChapterSelectionCommand = new Command<BibleBookListViewItemModel>(async x =>
            {
                ReduxContainer.Store.Dispatch(new ChapterSelectionAction()
                {
                    TentativeBibleReadingSchedule = new BibleReadingSchedule()
                    {
                        LanguageCode = tentative.LanguageCode,
                        PublicationCode = tentative.PublicationCode,
                        BookNumber = x.Number
                    }
                });

                var viewModel = IocSetup.Container.Resolve<ChapterSelectionViewModel>();
                await navigationService.Navigate(viewModel);

            });

            //set schedules from initial state.
            //this should fire only once 
            var subscription = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                   .Select(state => new { state.CurrentBibleReadingSchedule, state.TentativeBibleReadingSchedule })
                   .Where(x => x.CurrentBibleReadingSchedule != null && x.TentativeBibleReadingSchedule != null)
                   .DistinctUntilChanged()
                   .Take(1)
                   .Subscribe(async x =>
                   {
                       current = x.CurrentBibleReadingSchedule;
                       tentative = x.TentativeBibleReadingSchedule;
                       await initialize(tentative.LanguageCode, tentative.PublicationCode);
                   });

            disposables.Add(subscription);

            navigationService.NavigatedBack += onNavigated;
        }

        private void onNavigated(object viewModal)
        {
            if (viewModal.GetType() == this.GetType())
            {
                setSelectedBook();
            }
        }

        private void setSelectedBook()
        {
            if (current.LanguageCode == tentative.LanguageCode && current.PublicationCode == tentative.PublicationCode)
            {
                selectedBook = Books.FirstOrDefault(y => y.Number == current.BookNumber);
            }
            else
            {
                selectedBook = null;
            }

            RaiseProperty("SelectedBook");
        }
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ObservableHashSet<BibleBookListViewItemModel> Books { get; set; }
            = new ObservableHashSet<BibleBookListViewItemModel>();

        private BibleBookListViewItemModel selectedBook;
        public BibleBookListViewItemModel SelectedBook
        {
            get => selectedBook;
            set
            {
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
        }

        private async Task initialize(string languageCode, string publicationCode)
        {
            await populateBooks(languageCode, publicationCode);
        }

        private async Task populateBooks(string languageCode, string publicationCode)
        {
            IsBusy = true;
            Books.Clear();
            selectedBook = null;

            var books = await mediaService.GetBibleBooks(languageCode, publicationCode);
            foreach (var book in books.Select(x => x.Value))
            {
                var bookVM = new BibleBookListViewItemModel(book);
                Books.Add(bookVM);

                if (current.LanguageCode == tentative.LanguageCode
                    && current.PublicationCode == tentative.PublicationCode
                    && current.BookNumber == book.Number)
                {
                    selectedBook = bookVM;
                }
            }

            RaiseProperty("SelectedBook");
            IsBusy = true;
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }

    public class BibleBookListViewItemModel : IComparable
    {
        private readonly BibleBook book;
        public BibleBookListViewItemModel(BibleBook book)
        {
            this.book = book;
        }

        public string Name => book.Name;
        public int Number => book.Number;

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as BibleBookListViewItemModel).Number);
        }
    }
}
