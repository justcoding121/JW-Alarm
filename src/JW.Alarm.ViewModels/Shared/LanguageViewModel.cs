using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels
{
    public class LanguageViewModel : IComparable
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public LanguageViewModel(Language language)
        {
            Name = language.Name;
            Code = language.Code;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as LanguageViewModel).Name);
        }
    }
}
