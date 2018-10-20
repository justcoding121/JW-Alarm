﻿using Newtonsoft.Json;
using System;

namespace JW.Alarm.Models
{
    public class BibleChapter : IComparable
    {
        public int Number { get; set; }
        [JsonIgnore]
        public string Title => $"Chapter {Number}";
        public string Url { get; set; }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as BibleChapter).Number);
        }
    }
}
