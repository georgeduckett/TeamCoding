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
        private ImageSource _AvatarImageSource;
        public ImageSource AvatarImageSource
        {
            get { return _AvatarImageSource; }
            set
            {
                _AvatarImageSource = value;
                OnPropertyChanged(nameof(AvatarImageSource));
            }
        }
        private Brush _BorderBrush;
        public Brush BorderBrush
        {
            get { return _BorderBrush; }
            set
            {
                _BorderBrush = value;
                OnPropertyChanged(nameof(BorderBrush));
            }
        }
        private Visibility _BorderVisibility;
        public Visibility BorderVisibility
        {
            get { return _BorderVisibility; }
            set
            {
                _BorderVisibility = value;
                OnPropertyChanged(nameof(BorderVisibility));
            }
        }
        private string _Tag;
        public string Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }
        private string _ToolTip;
        public string ToolTip
        {
            get { return _ToolTip; }
            set
            {
                _ToolTip = value;
                OnPropertyChanged(nameof(ToolTip));
            }
        }
        private Brush _BackgroundBrush;
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set
            {
                _BackgroundBrush = value;
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }
        private char? _Letter;
        public char? Letter
        {
            get { return _Letter; }
            set
            {
                _Letter = value;
                OnPropertyChanged(nameof(Letter));
            }
        }
        private Brush _LetterBrush;
        public Brush LetterBrush
        {
            get { return _LetterBrush; }
            set
            {
                _LetterBrush = value;
                OnPropertyChanged(nameof(LetterBrush));
            }
        }
        private Thickness _LetterMargin;
        public Thickness LetterMargin
        {
            get { return _LetterMargin; }
            set
            {
                _LetterMargin = value;
                OnPropertyChanged(nameof(LetterMargin));
            }
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
