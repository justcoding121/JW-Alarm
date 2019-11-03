using Bible.Alarm.Models;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels
{
    public class NumberOfChaptersListViewItemModel : ViewModel, IComparable
    {
        public string Text => $"{Value} {(Value == 1 ? "Chapter" : "Chapters")}";
        public int Value { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public NumberOfChaptersListViewItemModel(int number)
        {
            Value = number;
        }

        public int CompareTo(object obj)
        {
            return Value.CompareTo((obj as NumberOfChaptersListViewItemModel).Value);
        }
    }
}
