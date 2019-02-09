using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels
{
    public class LanguageListViewItemModel : IComparable
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public LanguageListViewItemModel(Language language)
        {
            Name = language.Name;
            Code = language.Code;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as LanguageListViewItemModel).Name);
        }
    }
}
