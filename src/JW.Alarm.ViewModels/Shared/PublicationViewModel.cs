using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels
{
    public class PublicationViewModel : IComparable
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public PublicationViewModel(Publication publication)
        {
            Name = publication.Name;
            Code = publication.Code;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as PublicationViewModel).Name);
        }
    }
}
