using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeamCoding.Extensions
{
    public static class TextBlockExtensions
    {
        public static FormattedText GetFormattedText(this TextBlock textBlock)
        {
            return new FormattedText(textBlock.Text,
                                     System.Globalization.CultureInfo.CurrentUICulture,
                                     FlowDirection.LeftToRight,
                                     new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                                     textBlock.FontSize, textBlock.Foreground);
        }
        public static Rect GetBoundingRect(this TextBlock textBlock)
        {
            var text = textBlock.GetFormattedText();
            return text.BuildGeometry(new Point()).Bounds;
        }
    }
}
