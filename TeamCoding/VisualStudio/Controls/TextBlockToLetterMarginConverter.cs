using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Controls
{
    public class TextBlockToLetterMarginConverter : IMultiValueConverter
    { // This is a multi-value converter so we can add the text property as a binding so changing it triggers a re-calculation
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var rect = ((TextBlock)values[0]).GetBoundingRect();
            if (rect.Top >= 5)
            { // If we have a lot of blank space at the top of the up-most pixel of the rendered character (for lower case letters for example), move the text up
                return new Thickness(0, (-rect.Top) / 2, 0, 0);
            }
            else
            {
                return new Thickness(0);
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
