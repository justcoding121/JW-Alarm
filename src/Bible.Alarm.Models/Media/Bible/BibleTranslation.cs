﻿using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class BibleTranslation : TranslatedPublication
    {
        public int Id { get; set; }
        public List<BibleBook> Books { get; set; } = new List<BibleBook>();
    }
}