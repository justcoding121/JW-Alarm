using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class Language : IComparable
    {
        public int Id { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as Language).Name);
        }
    }
}
