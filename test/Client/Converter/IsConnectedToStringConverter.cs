using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converter
{
    public sealed class IsConnectedToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null
                || !(value is bool))
            {
                return null;
            }

            try
            {
                if ((bool)value == true)
                {
                    return "Disconnect";
                }

                return "Connect";
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
