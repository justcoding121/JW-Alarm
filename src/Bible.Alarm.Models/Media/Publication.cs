using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class Publication : IComparable
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as Publication).Name);
        }
    }
}
