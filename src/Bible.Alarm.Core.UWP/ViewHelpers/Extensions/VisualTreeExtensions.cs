using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace JW.Alarm.Core.Uwp
{
    public static class VisualTreeExtensions
    {
        public static T FindVisualAncestor<T>(this DependencyObject wpfObject) where T : DependencyObject
        {
            while (wpfObject != null)
            {
                if (wpfObject.GetType() == typeof(T))
                {
                    return (T)wpfObject;
                }

                wpfObject = VisualTreeHelper.GetParent(wpfObject);
            }

            return default(T);
        }
    }
}
