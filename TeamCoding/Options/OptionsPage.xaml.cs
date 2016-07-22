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

            foreach(var textBox in this.FindChildren<TextBox>())
            {
                textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
                textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            sbiDescription.Content = ((Control)sender).ToolTip;
            // TODO: Find the label with the same tool tip and set the title to it's caption
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            sbiDescription.Content = null;
        }
    }
}
