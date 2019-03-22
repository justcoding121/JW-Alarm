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
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
            var subscription1 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
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

            //set schedules from initial state.
            //this should fire only once 
            var subscription2 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                   .Select(state => state.CurrentBibleReadingSchedule)
                   .Where(x => x != null)
                   .DistinctUntilChanged()
                   .Subscribe(x =>
                   {
                       current = x;
                   });

            disposables.Add(subscription1);
            disposables.Add(subscription2);

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
                if(SelectedBook!=null)
                {
                    SelectedBook.IsSelected = false;
                }

                SelectedBook = Books.First(x => x.Number == current.BookNumber);
                SelectedBook.IsSelected = true;
  
            }
        }

        public BibleBookListViewItemModel SelectedBook { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        private ObservableCollection<BibleBookListViewItemModel> books;
        public ObservableCollection<BibleBookListViewItemModel> Books
        {
            get => books;
            set => this.Set(ref books, value);
        }

        private async Task initialize(string languageCode, string publicationCode)
        {
            await populateBooks(languageCode, publicationCode);
        }

        private async Task populateBooks(string languageCode, string publicationCode)
        {
            IsBusy = true;

            var books = await mediaService.GetBibleBooks(languageCode, publicationCode);
            var bookVMs = new ObservableCollection<BibleBookListViewItemModel>();

            foreach (var book in books.Select(x => x.Value))
            {
                var bookVM = new BibleBookListViewItemModel(book);
                bookVMs.Add(bookVM);

                if (current.LanguageCode == tentative.LanguageCode
                    && current.PublicationCode == tentative.PublicationCode
                    && current.BookNumber == book.Number)
                {
                    bookVM.IsSelected = true;
                    SelectedBook = bookVM;
                }
            }

            Books = bookVMs;

            IsBusy = false;
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }

    public class BibleBookListViewItemModel : ViewModel, IComparable
    {
        private readonly BibleBook book;
        public BibleBookListViewItemModel(BibleBook book)
        {
            this.book = book;
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public string Name => book.Name;
        public int Number => book.Number;

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as BibleBookListViewItemModel).Number);
        }
    }
}
