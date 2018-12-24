using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class BookSelectionViewModel
    {
        private BibleReadingSchedule current;
        private BibleReadingSchedule tentative;

        public BookSelectionViewModel(BibleReadingSchedule current, BibleReadingSchedule tentative)
        {
            this.current = current;
            this.tentative = tentative;
        }
    }
}
