using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace JW.Alarm.Services.UWP
{

    public class UwpMediaPlayService : MediaPlayService
    {
        private DownloadService downloadService;
        private IStorageService storageService;

        public UwpMediaPlayService(IAlarmScheduleService alarmscheduleService, 
            IBibleReadingScheduleService bibleReadingScheduleService,
            MediaService mediaService, DownloadService downloadService, IStorageService storageService)
            : base(alarmscheduleService, bibleReadingScheduleService, mediaService)
        {
            this.downloadService = downloadService;
            this.storageService = storageService;
        }

    }
}
