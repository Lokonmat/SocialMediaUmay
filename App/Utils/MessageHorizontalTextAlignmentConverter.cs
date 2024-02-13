using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App.Utils
{
    public class MessageHorizontalTextAlignmentConverter : IValueConverter
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
                    return TextAlignment.End;
                }
                else
                {
                    return TextAlignment.Start;
                }
            }
            return TextAlignment.Start; // Varsayılan değer
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
