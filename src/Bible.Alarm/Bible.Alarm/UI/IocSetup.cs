using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.Music;
using Bible.Alarm;
using Bible.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Bible.Alarm.UI.Views.General;

namespace Bible.Alarm.UI
{
    public static class IocSetup
    {
        public static void Initialize(IContainer container, bool isService)
        {
            if (!isService)
            {
                container.Register(x => new Schedule());

                container.Register(x => new MusicSelection());
                container.Register(x => new SongBookSelection());
                container.Register(x => new TrackSelection(container));

                container.Register(x => new BibleSelection());
                container.Register(x => new BookSelection(container));
                container.Register(x => new ChapterSelection(container));

                container.Register(x => new LanguageModal());

                container.Register(x => new AlarmModal());
                container.Register(x => new BatteryOptimizationExclusionModal());
                container.Register(x => new NumberOfChaptersModal());
                container.Register(x => new MediaProgressModal());
            }
        }
    }
}
