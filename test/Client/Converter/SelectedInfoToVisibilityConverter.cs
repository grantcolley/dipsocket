using Client.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converter
{
    public class SelectedInfoToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null
                && value is Channel)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
