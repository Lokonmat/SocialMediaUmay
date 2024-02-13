using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App.Utils
{
    public class MessageHorizontalConverter : IValueConverter
    {
        public string FullName
        {
            get { return Preferences.Get("FullName", ""); }
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string userName)
            {
                if (userName == FullName)
                {
                    return LayoutOptions.EndAndExpand;
                }
                else
                {
                    return LayoutOptions.StartAndExpand;
                }
            }
            return LayoutOptions.StartAndExpand; // Varsayılan değer
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
