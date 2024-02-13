﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace App.Utils
{
    public class IsPdfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string paylasimTuru)
            {
                return paylasimTuru == "pdf";
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
