using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class BookSelectionViewModel : ViewModelBase, IDisposable
    {
        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;

        private MediaService mediaService;
        private IThreadService threadService;
        private IPopUpService popUpService;

        public BookSelectionViewModel(BibleReadingSchedule current, BibleReadingSchedule tentative)
        {
            this.current = current;
            this.tentative = tentative;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();

            Refresh();
        }

        public ObservableHashSet<BibleBookListViewItemModel> Books { get; set; } = new ObservableHashSet<BibleBookListViewItemModel>();

        private BibleBookListViewItemModel selectedBook;
        public BibleBookListViewItemModel SelectedBook
        {
            get => selectedBook;
            set
            {
                selectedBook = value;
                RaiseProperty("SelectedBook");
            }
        }

        public ChapterSelectionViewModel GetChapterSelectionViewModel(BibleBookListViewItemModel selectedBook)
        {
            tentative.BookNumber = selectedBook.Number;
            return new ChapterSelectionViewModel(current, tentative);
        }

        public void Refresh()
        {
            Task.Run(() => InitializeAsync(tentative.LanguageCode, tentative.PublicationCode));
        }

        private async Task InitializeAsync(string languageCode, string publicationCode)
        {
            await populateBooks(languageCode, publicationCode);
        }

        private async Task populateBooks(string languageCode, string publicationCode)
        {
            await popUpService.ShowProgressRing();

            var books = await mediaService.GetBibleBooks(languageCode, publicationCode);

            await threadService.RunOnUIThread(() =>
            {
                Books.Clear();
            });

            foreach (var book in books.Select(x => x.Value))
            {
                var bookVM = new BibleBookListViewItemModel(book);

                await threadService.RunOnUIThread(() =>
                {
                    Books.Add(bookVM);
                });

                if (current.LanguageCode == tentative.LanguageCode
                    && current.PublicationCode == tentative.PublicationCode
                    && tentative.ChapterNumber == book.Number)
                {
                    selectedBook = bookVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedBook");
            });

            await popUpService.HideProgressRing();
        }

        public void Dispose()
        {

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
