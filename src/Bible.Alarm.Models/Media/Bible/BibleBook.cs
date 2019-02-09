using Advanced.Algorithms.DataStructures.Foundation;
using System;

namespace JW.Alarm.Models
{
    public class BibleBook : IComparable
    {
        public string Name { get; set; }
        public int Number { get; set; }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as BibleBook).Number);
        }
    }
}
