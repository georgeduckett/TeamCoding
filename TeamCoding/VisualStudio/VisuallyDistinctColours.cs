using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TeamCoding.VisualStudio
{
    public class VisuallyDistinctColours
    {
        private static readonly List<Color> Colours = new List<Color>
            {
                UIntToColor(0xFFFFB300), //Vivid Yellow
                UIntToColor(0xFF803E75), //Strong Purple
                UIntToColor(0xFFFF6800), //Vivid Orange
                UIntToColor(0xFFA6BDD7), //Very Light Blue
                UIntToColor(0xFFC10020), //Vivid Red
                UIntToColor(0xFFCEA262), //Grayish Yellow
                UIntToColor(0xFF817066), //Medium Gray
                UIntToColor(0xFF007D34), //Vivid Green
                UIntToColor(0xFFF6768E), //Strong Purplish Pink
                UIntToColor(0xFF00538A), //Strong Blue
                UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
                UIntToColor(0xFF53377A), //Strong Violet
                UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
                UIntToColor(0xFFB32851), //Strong Purplish Red
                UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
                UIntToColor(0xFF7F180D), //Strong Reddish Brown
                UIntToColor(0xFF93AA00), //Vivid Yellowish Green
                UIntToColor(0xFF593315), //Deep Yellowish Brown
                UIntToColor(0xFFF13A13), //Vivid Reddish Orange
                UIntToColor(0xFF232C16), //Dark Olive Green
            };
        private static Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }
        public static Color GetColourFromSeed(int seed)
        {
            return Colours[Math.Abs(seed % Colours.Count)];
        }

        public static Brush GetTextBrushFromBackgroundColour(Color backgroundColour)
        {
            var r = F(backgroundColour.R / 255.0);
            var g = F(backgroundColour.G / 255.0);
            var b = F(backgroundColour.B / 255.0);

            var l = 0.2126 * r + 0.7152 * g + 0.0722 * b;

            if (l > 0.179)
                return Brushes.Black;
            else
                return Brushes.White;
        }

        private static double F(double c)
        {
            if (c <= 0.03928)
            {
                c = c / 12.92;
            }
            else
            {
                c = Math.Pow((c + 0.055) / 1.055, 2.4f);
            }

            return c;
        }
    }
}
