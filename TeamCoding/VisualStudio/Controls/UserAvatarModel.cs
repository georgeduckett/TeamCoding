using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TeamCoding.VisualStudio.Controls
{
    public class UserAvatarModel : INotifyPropertyChanged
    {
        public ImageSource AvatarImageSource { get; set; }
        public Brush BorderBrush { get; set; }
        public Visibility BorderVisibility { get; set; }
        public string ToolTip { get; set; }
        public Brush BackgroundBrush { get; set; }
        public char? Letter { get; set; }
        public Brush LetterBrush { get; set; }
        public Thickness LetterMargin { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
