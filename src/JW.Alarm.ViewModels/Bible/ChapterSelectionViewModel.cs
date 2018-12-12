using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class ChapterSelectionViewModel
    {
        private BibleReadingSchedule current;
        public ChapterSelectionViewModel(BibleReadingSchedule current, BibleReadingSchedule model)
        {
            this.current = current;
        }
    }
}
