using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public const string chkShowSelfCaption = "Show yourself";
        public const string chkShowAllBranchesCaption = "Show All Branches";
        public const string cmdShowJsonExampleCaption = "Show example " + Settings.TeamCodingConfigFileName;

        private readonly Dictionary<TextBox, CancellationTokenSource> TextBoxIsValidTaskCancelSources = new Dictionary<TextBox, CancellationTokenSource>();

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

            string bindingPath;
            foreach(var textBox in grpPersistence.FindChildren<TextBox>())
            {
                bindingPath = textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.Path?.Path;
                
                if(bindingPath != null)
                {
                    TextBoxIsValidTaskCancelSources.Add(textBox, new CancellationTokenSource());
                    textBox.LostKeyboardFocus += PersistencePropertyBoundTextBox_KeyboardLostFocus;
                    textBox.TextChanged += PropertyBoundTextBox_TextChanged;
                }
            }

            bindingPath = txtUserImageUrl.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.Path?.Path;

            if (bindingPath != null)
            {
                TextBoxIsValidTaskCancelSources.Add(txtUserImageUrl, new CancellationTokenSource());
                txtUserImageUrl.LostKeyboardFocus += UserPropertyBoundTextBox_KeyboardLostFocus;
                txtUserImageUrl.TextChanged += PropertyBoundTextBox_TextChanged;
            }

            Loaded += OptionsPage_Loaded;
        }
        private void PropertyBoundTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var bindingPath = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding.Path.Path;
            
            var textBlock = (TextBlock)FindName("tb" + bindingPath);
            textBlock.Text = null;
            textBlock.ToolTip = null;
        }
        private void PersistencePropertyBoundTextBox_KeyboardLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            ReEvaluateTextboxSetting(textBox, TeamCodingPackage.Current.Settings.SharedSettings);
        }
        private void UserPropertyBoundTextBox_KeyboardLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            ReEvaluateTextboxSetting(textBox, TeamCodingPackage.Current.Settings.UserSettings);
        }

        private void ReEvaluateTextboxSetting(TextBox textBox, object settingsObject)
        {
            // Cancel any existing tasks to see if the new setting property is valid (since we're changing it anyway)
            TextBoxIsValidTaskCancelSources[textBox].Cancel();

            var bindingPath = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding.Path.Path;

            var textBlock = (TextBlock)FindName("tb" + bindingPath);
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBlock.Text = null;
                textBlock.ToolTip = null;
            }
            else
            {
                textBlock.Foreground = Brushes.Black;
                textBlock.Text = "⏳";
                textBlock.ToolTip = "Checking configuration value...";
                var settingProperty = (SettingProperty<string>)settingsObject.GetType().GetField(bindingPath + "Property", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(settingsObject);

                // Add a new token to the dictionary to use with this task we're about to create
                TextBoxIsValidTaskCancelSources[textBox] = new CancellationTokenSource();
                var isValidNewValueTask = settingProperty.GetNewValueInvalidReasonAsync(textBox.Text).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        textBlock.Foreground = Brushes.Red;
                        textBlock.Text = "❌";
                        textBlock.ToolTip = "An exception occurred checking the setting" + Environment.NewLine + Environment.NewLine + t.Exception.InnerException.ToString();
                    }
                    else if (t.Result == null)
                    {
                        textBlock.Foreground = Brushes.Green;
                        textBlock.Text = "✓";
                        textBlock.ToolTip = null;
                    }
                    else
                    {
                        textBlock.Foreground = Brushes.Red;
                        textBlock.Text = "❌";
                        textBlock.ToolTip = t.Result;
                    }
                }, TextBoxIsValidTaskCancelSources[textBox].Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        private void OptionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var loadedFromFile = TeamCodingPackage.Current?.Settings?.LoadFromJsonFile() ?? false;

            // After we've loaded the users settings override any Json settings
            if (loadedFromFile)
            {
                TeamCodingPackage.Current.Settings.UpdateToGrid((OptionPageGrid)DataContext);
            }

            foreach (var child in grpPersistence.Children().OfType<FrameworkElement>())
            {
                child.IsEnabled = !loadedFromFile;
            }

            chkUsingJsonSettings.IsChecked = loadedFromFile;

            string bindingPath;

            foreach (var textBox in grpPersistence.FindChildren<TextBox>())
            {
                bindingPath = textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.Path?.Path;

                if (bindingPath != null)
                {
                    textBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    ReEvaluateTextboxSetting(textBox, TeamCodingPackage.Current.Settings.SharedSettings);
                }
            }

            bindingPath = txtUserImageUrl.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.Path?.Path;

            if (bindingPath != null)
            {
                txtUserImageUrl.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                ReEvaluateTextboxSetting(txtUserImageUrl, TeamCodingPackage.Current.Settings.UserSettings);
            }
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
        private void CmdShowJsonExample_Click(object sender, RoutedEventArgs e)
        {
            new TeamCodingExample().ShowModal();
        }
    }
}
