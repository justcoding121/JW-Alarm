using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class BibleSelectionViewModel : ViewModelBase, IDisposable
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private IThreadService threadService;

        private readonly BibleReadingSchedule current;
        private readonly BibleReadingSchedule tentative;

        private List<IDisposable> subscriptions = new List<IDisposable>();

        public BibleSelectionViewModel(BibleReadingSchedule current, BibleReadingSchedule tentative)
        {
            this.current = current;
            this.tentative = tentative;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            initialize();
        }

        private void initialize()
        {
            Task.Run(() => initializeAsync(tentative.LanguageCode));
        }

        public BookSelectionViewModel GetBookSelectionViewModel(PublicationListViewItemModel selectedBible)
        {
            tentative.LanguageCode = selectedLanguage.Code;
            tentative.PublicationCode = selectedBible.Code;

            return new BookSelectionViewModel(current, tentative);
        }

        public ObservableHashSet<PublicationListViewItemModel> Translations { get; set; } = new ObservableHashSet<PublicationListViewItemModel>();
        public ObservableHashSet<LanguageListViewItemModel> Languages { get; } = new ObservableHashSet<LanguageListViewItemModel>();

        private LanguageListViewItemModel selectedLanguage;
        public LanguageListViewItemModel SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;
                RaiseProperty("SelectedLanguage");
            }
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
                selectedTranslation = value;
                RaiseProperty("SelectedTranslation");
            }
        }

        private async Task initializeAsync(string languageCode)
        {
            await populateLanguages();
            await populateTranslations(languageCode);

            var subscription1 = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                                    onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                    => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                                    handler => PropertyChanged += handler,
                                                    handler => PropertyChanged -= handler)
                                .Where(x => x.Key == "SelectedLanguage")
                                .Where(x => SelectedLanguage != null)
                                .Do(async x => await populateTranslations(SelectedLanguage.Code))
                                .Subscribe();

            subscriptions.Add(subscription1);

            var subscription2 = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                              onNextHandler => (object sender, PropertyChangedEventArgs e)
                                              => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                              handler => PropertyChanged += handler,
                                              handler => PropertyChanged -= handler)
                          .Where(x => x.Key == "LanguageSearchTerm")
                          .Do(async x => await populateLanguages(LanguageSearchTerm))
                          .Subscribe();

            subscriptions.Add(subscription2);
        }

        private async Task populateLanguages(string searchTerm = null)
        {
            await popUpService.ShowProgressRing();

            var languages = await mediaService.GetBibleLanguages();

            await threadService.RunOnUIThread(() =>
            {
                Languages.Clear();
            });

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var languageVM = new LanguageListViewItemModel(language);

                await threadService.RunOnUIThread(() => Languages.Add(languageVM));

                if (languageVM.Code == tentative.LanguageCode)
                {
                    selectedLanguage = languageVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedLanguage");
            });

            await popUpService.HideProgressRing();
        }

        private async Task populateTranslations(string languageCode)
        {
            await popUpService.ShowProgressRing();

            await threadService.RunOnUIThread(() =>
            {
                Translations.Clear();
            });

            var translations = await mediaService.GetBibleTranslations(languageCode);
            foreach (var translation in translations.Select(x => x.Value))
            {
                var translationVM = new PublicationListViewItemModel(translation);

                await threadService.RunOnUIThread(() => Translations.Add(translationVM));

                if (current.LanguageCode == languageCode
                    && current.PublicationCode == translation.Code)
                {
                    selectedTranslation = translationVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedTranslation");
            });

            await popUpService.HideProgressRing();
        }

        public void Dispose()
        {
            subscriptions.ForEach(x => x.Dispose());
        }
    }
}
