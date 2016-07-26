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
        private static readonly List<SolidColorBrush> Colours = new List<SolidColorBrush>
            {
                new SolidColorBrush(UIntToColor(0xFFFFB300)), //Vivid Yellow
                new SolidColorBrush(UIntToColor(0xFF803E75)), //Strong Purple
                new SolidColorBrush(UIntToColor(0xFFFF6800)), //Vivid Orange
                new SolidColorBrush(UIntToColor(0xFFA6BDD7)), //Very Light Blue
                new SolidColorBrush(UIntToColor(0xFFC10020)), //Vivid Red
                new SolidColorBrush(UIntToColor(0xFFCEA262)), //Grayish Yellow
                new SolidColorBrush(UIntToColor(0xFF817066)), //Medium Gray
                new SolidColorBrush(UIntToColor(0xFF007D34)), //Vivid Green
                new SolidColorBrush(UIntToColor(0xFFF6768E)), //Strong Purplish Pink
                new SolidColorBrush(UIntToColor(0xFF00538A)), //Strong Blue
                new SolidColorBrush(UIntToColor(0xFFFF7A5C)), //Strong Yellowish Pink
                new SolidColorBrush(UIntToColor(0xFF53377A)), //Strong Violet
                new SolidColorBrush(UIntToColor(0xFFFF8E00)), //Vivid Orange Yellow
                new SolidColorBrush(UIntToColor(0xFFB32851)), //Strong Purplish Red
                new SolidColorBrush(UIntToColor(0xFFF4C800)), //Vivid Greenish Yellow
                new SolidColorBrush(UIntToColor(0xFF7F180D)), //Strong Reddish Brown
                new SolidColorBrush(UIntToColor(0xFF93AA00)), //Vivid Yellowish Green
                new SolidColorBrush(UIntToColor(0xFF593315)), //Deep Yellowish Brown
                new SolidColorBrush(UIntToColor(0xFFF13A13)), //Vivid Reddish Orange
                new SolidColorBrush(UIntToColor(0xFF232C16)), //Dark Olive Green
            };
        static public Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }
        public static SolidColorBrush GetRandomColour(int seed)
        {
            return Colours[seed % Colours.Count];
        }
    }
}
