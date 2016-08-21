using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeamCoding.Documents;
using TeamCoding.IdentityManagement;
using TeamCoding.Extensions;
using Microsoft.VisualStudio.Shell;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Maintains a cache of images of users
    /// </summary>
    public class UserImageCache
    {
        private static readonly Brush DocSelectedBorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        private static readonly Brush DocEditedBorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 0.5f, ScG = 0.5f, ScB = 0.5f });
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();
        public Panel CreateUserIdentityControl(UserIdentity userIdentity, bool withBorder = false)
        {
            var firstLetter = userIdentity.Id[0];
            var grid = new Grid();
            Image imageControl;
            grid.Children.Add(new Viewbox()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                Child = new TextBlock()
                {
                    LineStackingStrategy = LineStackingStrategy.MaxHeight,
                    TextAlignment = TextAlignment.Center,
                    TextTrimming = TextTrimming.None,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            });
            grid.Children.Add(imageControl = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            if (withBorder)
            {
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = UserColours.GetUserBrush(userIdentity);
            }
            grid.Children.Add(border);

            if (userIdentity.ImageUrl != null)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    try
                    {
                        var request = await TeamCodingPackage.Current.HttpClient.GetAsync(userIdentity.ImageUrl);
                        if (!request.IsSuccessStatusCode) return;
                        var imageStream = await request.Content.ReadAsStreamAsync();
                        imageControl.Source = UrlImages[userIdentity.ImageUrl] = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
                    {
                        TeamCodingPackage.Current.Logger.WriteError(ex);
                    }
                });
            }
            return grid;
        }
        internal void SetUserControlProperties(Panel parentControl, RemotelyAccessedDocumentData matchedRemoteDoc)
        {
            var textBlockControl = parentControl.FindChild<TextBlock>();
            var firstLetter = (matchedRemoteDoc.IdeUserIdentity.Id)[0];
            textBlockControl.Text = firstLetter.ToString();
            parentControl.Background = UserColours.GetUserBrush(matchedRemoteDoc.IdeUserIdentity);

            var textBlockFormattedText = textBlockControl.GetBoundingRect();
            if (textBlockFormattedText.Top >= 5)
            { // If we have a lot of blank space at the top of the up-most pixel of the rendered character (for lower case letters for example), move the text up
                textBlockControl.Margin = new Thickness(0, (-textBlockFormattedText.Top) / 2, 0, 0);
            }
            else
            {
                textBlockControl.Margin = new Thickness(0);
            }

            parentControl.ToolTip = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id) + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty) + matchedRemoteDoc.CaretPositionInfo;

            if (matchedRemoteDoc.HasFocus)
            {
                var border = parentControl.FindChild<Border>();
                border.Visibility = Visibility.Visible;
                border.BorderBrush = DocSelectedBorderBrush;
            }
            else if (matchedRemoteDoc.BeingEdited)
            {
                var border = parentControl.FindChild<Border>();
                border.Visibility = Visibility.Visible;
                border.BorderBrush = DocEditedBorderBrush;
            }
            else
            {
                parentControl.Children.OfType<Border>().Single().Visibility = Visibility.Hidden;
            }

            SetImageSource(parentControl, matchedRemoteDoc);
        }

        private void SetImageSource(Panel parentControl, RemotelyAccessedDocumentData matchedRemoteDoc)
        {
            ImageSource imageSource = null;
            if (matchedRemoteDoc.IdeUserIdentity.ImageBytes != null)
            {
                using (var MS = new MemoryStream(matchedRemoteDoc.IdeUserIdentity.ImageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = MS;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imageSource = bitmap;
                }
            }
            else if (matchedRemoteDoc.IdeUserIdentity.ImageUrl != null)
            {
                if (UrlImages.ContainsKey(matchedRemoteDoc.IdeUserIdentity.ImageUrl))
                {
                    imageSource = UrlImages[matchedRemoteDoc.IdeUserIdentity.ImageUrl];
                }
            }

            parentControl.FindChild<Image>().Source = imageSource;
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        private static BitmapImage LoadBitmapFromResource(string pathInApplication)
        { // http://stackoverflow.com/a/9737958
            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + System.Reflection.Assembly.GetCallingAssembly().GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
        }
    }
}
