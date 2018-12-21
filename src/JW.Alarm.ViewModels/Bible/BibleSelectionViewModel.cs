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
    public class BibleSelectionViewModel : ViewModelBase
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private IThreadService threadService;

        private readonly BibleReadingSchedule model;

        public BibleSelectionViewModel(BibleReadingSchedule model)
        {
            this.model = model;
            languageCode = model.LanguageCode;
            publicationCode = model.PublicationCode;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            Task.Run(() => InitializePublicationsAsync(model.LanguageCode));
        }

        public ObservableHashSet<PublicationViewModel> Translations { get; } = new ObservableHashSet<PublicationViewModel>();
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

        private async Task InitializePublicationsAsync(string languageCode)
        {
            await populateLanguages();
            await populateTranslations(languageCode);

            var subscription = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                                    onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                    => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                                    handler => PropertyChanged += handler,
                                                    handler => PropertyChanged -= handler)
                                .Where(x => x.Key == "SelectedLanguage")
                                .Select(x => (x.Value as BibleSelectionViewModel).SelectedLanguage)
                                .Do(async x => await populateTranslations(x.Code))
                                .Subscribe();
        }

        private async Task populateLanguages()
        {
            var languages = await mediaService.GetBibleLanguages();

            foreach (var language in languages)
            {
                await threadService.RunOnUIThread(() =>
                {
                    var languageVM = new LanguageViewModel(language.Value);
                    Languages.Add(languageVM);
                    if (language.Value.Code == languageCode)
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
            foreach (var translation in translations)
            {
                var translationVM = new PublicationViewModel(translation.Value);
                await threadService.RunOnUIThread(() => Translations.Add(translationVM));
                if(translationVM.Code == publicationCode)
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
    }
}
