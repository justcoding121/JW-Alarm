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
    public class SongBookSelectionViewModel : ViewModelBase
    {
        private MediaService mediaService;
        private IPopUpService popUpService;

        private readonly AlarmMusic model;

        private Music current;

        public SongBookSelectionViewModel(Music current, AlarmMusic model)
        {
            this.current = current;

            this.model = model;

            publicationCode = model.PublicationCode;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();

            Task.Run(() => InitializePublicationsAsync(model.LanguageCode));
        }

        public ObservableHashSet<Publication> Translations { get; } = new ObservableHashSet<Publication>();

        private string publicationCode;
        public string PublicationCode
        {
            get => publicationCode;
            set => this.Set(ref publicationCode, value);
        }

        private async Task InitializePublicationsAsync(string languageCode)
        {
            await populateSongBooks(languageCode);

            var subscription = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                                    onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                    => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                                    handler => PropertyChanged += handler,
                                                    handler => PropertyChanged -= handler)
                                .Where(x => x.Key == "LanguageCode")
                                .Select(x => x.Value as Language)
                                .Do(async x => await populateSongBooks(x.Code));
        }

        private async Task populateSongBooks(string languageCode)
        {
            await popUpService.ShowProgressRing();


            await popUpService.HideProgressRing();
        }
    }
}
