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
        public UserAvatar(bool forDocumentTab)
            : this()
        {
            if (forDocumentTab)
            {
                // For a user document we don't bind the outer border as it's set according to whether the user is focusing on / editing the document or not
                BindingOperations.ClearBinding(bdrOuterBorder, VisibilityProperty);
                BindingOperations.ClearBinding(bdrOuterBorder, BorderBrushProperty);
            }
        }
    }
}
