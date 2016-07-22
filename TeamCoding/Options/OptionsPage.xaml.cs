using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeamCoding.Extensions;

namespace TeamCoding.Options
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : UserControl
    {
        public OptionsPage(OptionPageGrid optionPageGrid)
        {
            InitializeComponent();
            DataContext = optionPageGrid;

            foreach (var textBox in this.FindChildren<TextBox>())
            {
                textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
                textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
            }
        }
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var senderTooltip = ((Control)sender).ToolTip;
            sbiDescription.Content = senderTooltip;
            sbiTitle.Content = (AutomationProperties.GetLabeledBy(sender as DependencyObject) as Label).Content;
        }
        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            sbiTitle.Content = sbiDescription.Content = null;
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for and read in a config file at the route of this solution's repo.
            // TODO: When changing solutions if Settings.AllowRepoConfigToOverwriteSettings is set then load new settings if a new config file exists for the new repo.
            txtUsername.Text = Settings.DefaultUsername;
            txtUsername.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            txtUserImageUrl.Text = Settings.DefaultImageUrl;
            txtUserImageUrl.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            txtFileBasedPersisterPath.Text = Settings.DefaultFileBasedPersisterPath;
            txtFileBasedPersisterPath.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}
