using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converter
{
    public sealed class BoolToConnectConverter : IValueConverter
    {
        private ResourceDictionary resourceDictionary;

        public BoolToConnectConverter()
        {
            ResourceDictionary = new ResourceDictionary();
        }

        public ResourceDictionary ResourceDictionary
        {
            get { return resourceDictionary; }
            set { resourceDictionary = value; }
        }

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
                    return ResourceDictionary["disconnect"];
                }

                return ResourceDictionary["connect"];
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
