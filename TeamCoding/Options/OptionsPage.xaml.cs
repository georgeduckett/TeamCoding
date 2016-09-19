﻿using System;
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
        public const string chkUsingJsonSettingsCaption = "Using " + Settings.TeamCodingConfigFileName;
        public const string cmdShowJsonExampleCaption = "Show example " + Settings.TeamCodingConfigFileName;

        public OptionsPage(OptionPageGrid optionPageGrid)
        {
            InitializeComponent();
            DataContext = optionPageGrid;

            foreach (var textBox in this.FindChildren<TextBox>())
            {
                textBox.GotKeyboardFocus += Control_GotKeyboardFocus;
                textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
            }
            foreach(var comboBox in this.FindChildren<ComboBox>())
            {
                comboBox.GotKeyboardFocus += Control_GotKeyboardFocus;
                comboBox.LostKeyboardFocus += ComboBox_LostKeyboardFocus;
            }

            Loaded += OptionsPage_Loaded;
        }
        private void OptionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var loadedFromFile = TeamCodingPackage.Current?.Settings?.LoadFromJsonFile() ?? false;

            foreach (var child in grpPersistence.Children().OfType<FrameworkElement>())
            {
                child.IsEnabled = !loadedFromFile;
            }

            chkUsingJsonSettings.IsChecked = loadedFromFile;
        }
        private void Control_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var senderTooltip = ((Control)sender).ToolTip;
            sbitxtDescription.Text = senderTooltip.ToString();
            sbiTitle.Content = (AutomationProperties.GetLabeledBy(sender as DependencyObject) as Label).Content;
        }
        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            sbiTitle.Content = sbitxtDescription.Text = null;
        }
        private void ComboBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
            sbiTitle.Content = sbitxtDescription.Text = null;
        }
        private void cmdShowJsonExample_Click(object sender, RoutedEventArgs e)
        {
            new TeamCodingExample().ShowModal();
        }
    }
}
