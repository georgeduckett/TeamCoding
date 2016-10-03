using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeamCoding.VisualStudio.Controls
{
    /// <summary>
    /// Interaction logic for UserAvatar.xaml
    /// </summary>
    public partial class UserAvatar : UserControl
    {
        public UserAvatarModel Model => (UserAvatarModel)DataContext;
        public UserAvatar()
        {
            InitializeComponent();
        }
        public Visibility UserBorderVisibility
        {
            get
            {
                return bdrOuterBorder.Visibility;
            }
            set
            {
                BindingOperations.ClearBinding(bdrOuterBorder, VisibilityProperty);
                bdrOuterBorder.Visibility = value;
            }
        }

        public Brush UserBorderBrush
        {
            get
            {
                return bdrOuterBorder.BorderBrush;
            }
            set
            {
                BindingOperations.ClearBinding(bdrOuterBorder, BorderBrushProperty);
                bdrOuterBorder.BorderBrush = value;
            }
        }
    }
}
