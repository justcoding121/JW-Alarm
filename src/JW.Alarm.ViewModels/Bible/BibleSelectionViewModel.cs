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

        private readonly BibleReadingSchedule model;

        private List<IDisposable> subscriptions = new List<IDisposable>();

        public BibleSelectionViewModel(BibleReadingSchedule model)
        {
            this.model = model;
            languageCode = model.LanguageCode;
            publicationCode = model.PublicationCode;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            Task.Run(() => InitializeAsync(model.LanguageCode));
        }

        public ObservableHashSet<PublicationViewModel> Translations { get; set; } = new ObservableHashSet<PublicationViewModel>();
        public ObservableHashSet<LanguageViewModel> Languages { get; } = new ObservableHashSet<LanguageViewModel>();

        private string languageCode;
        private LanguageViewModel selectedLanguage;
        public LanguageViewModel SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;
                RaiseProperty("SelectedLanguage");
            }
        }

        private string publicationCode;
        public string PublicationCode
        {
            get => publicationCode;
            set => this.Set(ref publicationCode, value);
        }

        private string languageSearchTerm;
        public string LanguageSearchTerm
        {
            get => languageSearchTerm;
            set => this.Set(ref languageSearchTerm, value);
        }

        private PublicationViewModel selectedTranslation;
        public PublicationViewModel SelectedTranslation
        {
            get => selectedTranslation;
            set
            {
                selectedTranslation = value;
                RaiseProperty("SelectedTranslation");
            }
        }

        private async Task InitializeAsync(string languageCode)
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
            var languages = await mediaService.GetBibleLanguages();

            await threadService.RunOnUIThread(() => Languages.Clear());

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                await threadService.RunOnUIThread(() =>
                {
                    var languageVM = new LanguageViewModel(language);
                    Languages.Add(languageVM);
                    if (languageVM.Code == languageCode)
                    {
                        selectedLanguage = languageVM;
                    }
                });
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedLanguage");
            });

        }

        private async Task populateTranslations(string languageCode)
        {
            await popUpService.ShowProgressRing();

            await threadService.RunOnUIThread(() => Translations.Clear());

            var translations = await mediaService.GetBibleTranslations(languageCode);
            foreach (var translation in translations.Select(x => x.Value))
            {
                var translationVM = new PublicationViewModel(translation);
                await threadService.RunOnUIThread(() => Translations.Add(translationVM));
                if (this.languageCode == languageCode 
                    && translationVM.Code == publicationCode)
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
