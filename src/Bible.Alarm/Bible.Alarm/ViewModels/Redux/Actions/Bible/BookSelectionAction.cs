﻿using JW.Alarm.Models;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions.Bible
{
    public class BookSelectionAction : IAction
    {
        public BibleReadingSchedule TentativeBibleReadingSchedule { get; set; }
    }
}