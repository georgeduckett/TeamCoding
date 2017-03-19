using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TeamCoding.VisualStudio.Controls
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class CustomisableBoolToVisibilityConverter : IValueConverter
    { // http://stackoverflow.com/questions/3128023
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public CustomisableBoolToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
                return true;
            if (Equals(value, FalseValue))
                return false;
            return null;
        }
    }
}
