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

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Maintains a cache of images of users
    /// </summary>
    public class UserImageCache // TODO: If we go with redis as a messaging option, maybe make it also a user image caching option
    {
        private static readonly List<SolidColorBrush> VisuallyDistinctColours = new List<SolidColorBrush>
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
        private static readonly Brush BorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        //private readonly BitmapImage SharedUnknownUserImage = LoadBitmapFromResource("Resources/UnknownUserImage.png");
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();
        private readonly IDEWrapper IdeWrapper;

        public UserImageCache(IDEWrapper ideWrapper)
        {
            IdeWrapper = ideWrapper;
        }
        private static Panel CreateUserIdentityControl(UserIdentity userIdentity)
        {
            var firstLetter = userIdentity.Id[0];
            var grid = new Grid()
            {
                Background = VisuallyDistinctColours[firstLetter % VisuallyDistinctColours.Count]
            };

            grid.Children.Add(new Viewbox()
            { // TODO: use http://stackoverflow.com/questions/9785322/textblock-as-big-as-a-capital-letter-ignoring-font-ascender-descender#15876463 to centre lower-case letters properly
                // TODO: figure out why background changes when changing name
                // TODO: when clearing name (or adding in?) make sure image can be replaced with control and visa-versa
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                Child = new TextBlock()
                {
                    LineHeight = 8f,
                    LineStackingStrategy = LineStackingStrategy.MaxHeight,
                    Text = firstLetter.ToString(),
                    TextAlignment = TextAlignment.Center,
                    TextTrimming = TextTrimming.None,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            });
            grid.Children.Add(new Border()
            {
                BorderBrush = BorderBrush,
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });

            return grid;
        }
        private static void UpdateIdentityControl(Panel parentControl, SourceControlledDocumentData matchedRemoteDoc)
        {
            var contentControl = parentControl.FindChild<ContentControl>();

            if (contentControl != null)
            {
                contentControl.Content = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id)[0];
            }
        }
        private static void UpdateUserImageControl(Panel parentControl, SourceControlledDocumentData matchedRemoteDoc)
        {
            parentControl.ToolTip = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id) + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);

            if (matchedRemoteDoc.HasFocus)
            {
                parentControl.Children.OfType<Border>().Single().Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                parentControl.Children.OfType<Border>().Single().Visibility = System.Windows.Visibility.Hidden;
            }
        }
        private static Panel CreateUserImageControl(ImageSource imageSource)
        {
            var grid = new Grid();

            grid.Children.Add(new Image()
            {
                Source = imageSource,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            grid.Children.Add(new Border()
            {
                BorderBrush = BorderBrush,
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });

            return grid;
        }
        public Panel GetUserImageControlFromUserIdentity(UserIdentity userIdentity, int desiredSize)
        {
            Panel result;

            if (userIdentity.ImageBytes != null)
            {
                using (var MS = new MemoryStream(userIdentity.ImageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = MS;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    result = CreateUserImageControl(bitmap);
                }

            }
            else if (userIdentity.ImageUrl != null)
            {
                if (UrlImages.ContainsKey(userIdentity.ImageUrl))
                {
                    return CreateUserImageControl(UrlImages[userIdentity.ImageUrl]);
                }

                result = CreateUserIdentityControl(userIdentity);
            }
            else
            {
                return CreateUserIdentityControl(userIdentity);
            }

            if (userIdentity.ImageUrl != null)
            {
                IdeWrapper.InvokeAsync(async () =>
                {
                    try
                    {
                        var request = await TeamCodingPackage.Current.HttpClient.GetAsync(userIdentity.ImageUrl);
                        if (!request.IsSuccessStatusCode) return;
                        var imageStream = await request.Content.ReadAsStreamAsync();
                        result.Children.OfType<Image>().Single().Source = UrlImages[userIdentity.ImageUrl] = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    catch { } // Any failures don't matter, it just won't update the image
            });
            }

            return result;
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
        internal void SetImageProperties(Panel parentControl, SourceControlledDocumentData matchedRemoteDoc)
        {
            UpdateIdentityControl(parentControl, matchedRemoteDoc);
            UpdateUserImageControl(parentControl, matchedRemoteDoc);
        }
    }
}
