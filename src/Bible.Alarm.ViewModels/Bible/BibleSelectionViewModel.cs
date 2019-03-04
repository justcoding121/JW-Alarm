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
    public class BibleSelectionViewModel : ViewModel, IDisposable
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private INavigationService navigationService;

        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;

        private List<IDisposable> disposables = new List<IDisposable>();

        public ICommand BackCommand { get; set; }
        public ICommand BookSelectionCommand { get; set; }
        public ICommand OpenModalCommand { get; set; }
        public ICommand CloseModalCommand { get; set; }
        public ICommand SelectLanguageCommand { get; set; }
        public ICommand SelectSongBookCommand { get; set; }

        public BibleSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

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

                     await initialize(tentative.LanguageCode);
                 });

            disposables.Add(subscription1);

            var subscription2 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
             .Select(state => new { state.CurrentBibleReadingSchedule, state.TentativeBibleReadingSchedule })
             .Where(x => x.CurrentBibleReadingSchedule != null && x.TentativeBibleReadingSchedule != null)
             .DistinctUntilChanged()
             .Skip(1)
             .Subscribe(x =>
             {
                 current = x.CurrentBibleReadingSchedule;
                 tentative = x.TentativeBibleReadingSchedule;
             });

            disposables.Add(subscription2);

            BookSelectionCommand = new Command<PublicationListViewItemModel>(async x =>
            {
                ReduxContainer.Store.Dispatch(new BookSelectionAction()
                {
                    TentativeBibleReadingSchedule = new BibleReadingSchedule()
                    {
                        PublicationCode = x.Code,
                        LanguageCode = selectedLanguage.Code
                    }
                });
                var viewModel = IocSetup.Container.Resolve<BookSelectionViewModel>();
                await navigationService.Navigate(viewModel);
            });

            OpenModalCommand = new Command(async () =>
            {
                await navigationService.ShowModal("LanguageModal", this);
            });

            BackCommand = new Command(async () =>
            {
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
            });

            CloseModalCommand = new Command(async () =>
            {
                await navigationService.CloseModal();
            });

            SelectLanguageCommand = new Command<LanguageListViewItemModel>(async x =>
            {
                selectedLanguage = x;
                RaiseProperty("SelectedLanguage");
                await navigationService.CloseModal();
                await populateTranslations(x.Code);
            });

            navigationService.NavigatedBack += onNavigated;
        }

        private void onNavigated(object viewModal)
        {
            if (viewModal.GetType() == this.GetType())
            {
                setSelectedTranslation();
            }
        }

        private void setSelectedTranslation()
        {
            if (current.LanguageCode == tentative.LanguageCode)
            {
                selectedTranslation = Translations.FirstOrDefault(y => y.Code == current.PublicationCode);
            }
            else
            {
                selectedTranslation = null;
            }

            RaiseProperty("SelectedTranslation");
        }

        public ObservableHashSet<PublicationListViewItemModel> Translations { get; set; } = new ObservableHashSet<PublicationListViewItemModel>();
        public ObservableHashSet<LanguageListViewItemModel> Languages { get; } = new ObservableHashSet<LanguageListViewItemModel>();

        private LanguageListViewItemModel selectedLanguage;
        public LanguageListViewItemModel SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public string PublicationCode
        {
            get => tentative.PublicationCode;
            set => this.Set(tentative.PublicationCode, value);
        }

        private string languageSearchTerm;
        public string LanguageSearchTerm
        {
            get => languageSearchTerm;
            set => this.Set(ref languageSearchTerm, value);
        }

        private PublicationListViewItemModel selectedTranslation;
        public PublicationListViewItemModel SelectedTranslation
        {
            get => selectedTranslation;
            set
            {
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
        }

        private async Task initialize(string languageCode)
        {
            await populateLanguages();
            await populateTranslations(languageCode);

            var subscription2 = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                              onNextHandler => (object sender, PropertyChangedEventArgs e)
                                              => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                              handler => PropertyChanged += handler,
                                              handler => PropertyChanged -= handler)
                          .Where(x => x.Key == "LanguageSearchTerm")
                          .Do(async x => await populateLanguages(LanguageSearchTerm))
                          .Subscribe();

            disposables.Add(subscription2);
        }

        private async Task populateLanguages(string searchTerm = null)
        {
            IsBusy = true;
            var languages = await mediaService.GetBibleLanguages();
            Languages.Clear();

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var languageVM = new LanguageListViewItemModel(language);

                Languages.Add(languageVM);

                if (languageVM.Code == tentative.LanguageCode)
                {
                    selectedLanguage = languageVM;
                }
            }

            RaiseProperty("SelectedLanguage");
            IsBusy = false;
        }

        private async Task populateTranslations(string languageCode)
        {
            IsBusy = true;
            Translations.Clear();
            SelectedTranslation = null;

            var translations = await mediaService.GetBibleTranslations(languageCode);
            foreach (var translation in translations.Select(x => x.Value))
            {
                var translationVM = new PublicationListViewItemModel(translation);

                Translations.Add(translationVM);

                if (current.LanguageCode == languageCode
                    && current.PublicationCode == translation.Code)
                {
                    selectedTranslation = translationVM;
                }
            }

            RaiseProperty("SelectedTranslation");
            IsBusy = false;
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }
}
