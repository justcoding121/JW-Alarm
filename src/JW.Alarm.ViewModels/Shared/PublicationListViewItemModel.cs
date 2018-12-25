using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels
{
    public class PublicationListViewItemModel : IComparable
    {
        private readonly Publication publication;

        public PublicationListViewItemModel(Publication publication)
        {
            this.publication = publication;
        }

        public string Name => publication.Name;
        public string Code => publication.Code;

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as PublicationListViewItemModel).Name);
        }
    }
}
