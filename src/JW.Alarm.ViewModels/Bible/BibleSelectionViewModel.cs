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

        private readonly BibleReadingSchedule model;
        public BibleSelectionViewModel(BibleReadingSchedule model)
        {
            this.model = model;
            languageCode = model.LanguageCode;
            publicationCode = model.PublicationCode;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();

            Task.Run(() => InitializePublicationsAsync(model.LanguageCode));
        }

        public void Navigated()
        {
            LanguageCode = model.LanguageCode;
            PublicationCode = model.PublicationCode;
        }

        public ObservableHashSet<Publication> Translations { get; } = new ObservableHashSet<Publication>();

        private string languageCode;
        public string LanguageCode
        {
            get => languageCode;
            set => this.Set(ref languageCode, value);
        }

        private string publicationCode;
        public string PublicationCode
        {
            get => publicationCode;
            set => this.Set(ref publicationCode, value);
        }

        private async Task InitializePublicationsAsync(string languageCode)
        {
            await populateTranslations(languageCode);

            var subscription = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                                    onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                    => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                                    handler => PropertyChanged += handler,
                                                    handler => PropertyChanged -= handler)
                                .Where(x => x.Key == "LanguageCode")
                                .Select(x => x.Value as Language)
                                .Do(async x => await populateTranslations(x.Code));
        }

        private async Task populateTranslations(string languageCode)
        {
            await popUpService.ShowProgressRing();

            var translations = await mediaService.GetBibleTranslations(languageCode);
            foreach (var translation in translations)
            {
                Translations.Add(translation.Value);
            }

            await popUpService.HideProgressRing();
        }
    }
}
