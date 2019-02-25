using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class BibleSelectionAction : IAction
    {
        public BibleSelectionViewModel BibleSelectionViewModel { get; set; }
    }
}
