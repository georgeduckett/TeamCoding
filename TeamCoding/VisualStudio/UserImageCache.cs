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

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Maintains a cache of images of users
    /// </summary>
    public class UserImageCache // TODO: If we go with redis as a messaging option, maybe make it also a user image caching option
    {
        private static readonly List<System.Drawing.Brush> VisuallyDistinctColours = new List<System.Drawing.Brush>
            {
                new System.Drawing.SolidBrush(UIntToColor(0xFFFFB300)), //Vivid Yellow
                new System.Drawing.SolidBrush(UIntToColor(0xFF803E75)), //Strong Purple
                new System.Drawing.SolidBrush(UIntToColor(0xFFFF6800)), //Vivid Orange
                new System.Drawing.SolidBrush(UIntToColor(0xFFA6BDD7)), //Very Light Blue
                new System.Drawing.SolidBrush(UIntToColor(0xFFC10020)), //Vivid Red
                new System.Drawing.SolidBrush(UIntToColor(0xFFCEA262)), //Grayish Yellow
                new System.Drawing.SolidBrush(UIntToColor(0xFF817066)), //Medium Gray
                new System.Drawing.SolidBrush(UIntToColor(0xFF007D34)), //Vivid Green
                new System.Drawing.SolidBrush(UIntToColor(0xFFF6768E)), //Strong Purplish Pink
                new System.Drawing.SolidBrush(UIntToColor(0xFF00538A)), //Strong Blue
                new System.Drawing.SolidBrush(UIntToColor(0xFFFF7A5C)), //Strong Yellowish Pink
                new System.Drawing.SolidBrush(UIntToColor(0xFF53377A)), //Strong Violet
                new System.Drawing.SolidBrush(UIntToColor(0xFFFF8E00)), //Vivid Orange Yellow
                new System.Drawing.SolidBrush(UIntToColor(0xFFB32851)), //Strong Purplish Red
                new System.Drawing.SolidBrush(UIntToColor(0xFFF4C800)), //Vivid Greenish Yellow
                new System.Drawing.SolidBrush(UIntToColor(0xFF7F180D)), //Strong Reddish Brown
                new System.Drawing.SolidBrush(UIntToColor(0xFF93AA00)), //Vivid Yellowish Green
                new System.Drawing.SolidBrush(UIntToColor(0xFF593315)), //Deep Yellowish Brown
                new System.Drawing.SolidBrush(UIntToColor(0xFFF13A13)), //Vivid Reddish Orange
                new System.Drawing.SolidBrush(UIntToColor(0xFF232C16)), //Dark Olive Green
            };
        static public System.Drawing.Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }
        private static readonly Brush BorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        //private readonly BitmapImage SharedUnknownUserImage = LoadBitmapFromResource("Resources/UnknownUserImage.png");
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();
        private readonly IDEWrapper IdeWrapper;

        public UserImageCache(IDEWrapper ideWrapper)
        {
            IdeWrapper = ideWrapper;
        }
        private BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap src)
        { // http://stackoverflow.com/a/34590774
            using (var ms = new MemoryStream())
            {
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage image = new BitmapImage();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.BeginInit();
                ms.Position = 0;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        public System.Drawing.Font GetAdjustedFont(System.Drawing.Graphics GraphicRef, string GraphicString, System.Drawing.Font OriginalFont, int ContainerSize, int MaxFontSize, int MinFontSize, bool SmallestOnFail)
        {
            System.Drawing.Font testFont = null;
            // We utilize MeasureString which we get via a control instance           
            for (int AdjustedSize = MaxFontSize; AdjustedSize >= MinFontSize; AdjustedSize--)
            {
                testFont = new System.Drawing.Font(OriginalFont.Name, AdjustedSize, OriginalFont.Style);

                // Test the string with the new size
                System.Drawing.SizeF AdjustedSizeNew = GraphicRef.MeasureString(GraphicString, testFont);

                if (ContainerSize > Convert.ToInt32(AdjustedSizeNew.Width) && ContainerSize > Convert.ToInt32(AdjustedSizeNew.Height))
                {
                    // Good font, return it
                    return testFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return MinimumSize or Original?
            if (SmallestOnFail)
            {
                return testFont;
            }
            else
            {
                return OriginalFont;
            }
        }
        private ImageSource CreatePlaceholderImage(UserIdentity identity, int desiredSize)
        {
            if (UrlImages.ContainsKey(identity.DisplayName))
            {
                return UrlImages[identity.DisplayName];
            }

            var result = new System.Drawing.Bitmap(desiredSize, desiredSize);
            using (var g = System.Drawing.Graphics.FromImage(result))
            {
                var firstLetter = (identity.DisplayName?[0] ?? identity?.Id?[0]).Value;

                g.FillRectangle(VisuallyDistinctColours[firstLetter % VisuallyDistinctColours.Count], new System.Drawing.Rectangle(0, 0, result.Width, result.Height));
                var FontSize = GetAdjustedFont(g, firstLetter.ToString(), new System.Drawing.Font("Courier New", 18f), result.Width, 18, 1, true);
                
                // Draw the first letter on the user image
                var letterSize = g.MeasureString(firstLetter.ToString(), FontSize);
                g.DrawString(firstLetter.ToString(), FontSize, System.Drawing.Brushes.White, (result.Width - letterSize.Width) / 2, (result.Height - letterSize.Height) / 2);
            }
            // TODO: Why is this image blank? - Maybe we should instead create a drawing element that renders the background colour and text automatically
            return UrlImages[identity.DisplayName] = ConvertToBitmapImage(result);
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

                result = CreateUserImageControl(CreatePlaceholderImage(userIdentity, desiredSize));
            }
            else
            {
                return CreateUserImageControl(CreatePlaceholderImage(userIdentity, desiredSize));
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
            var imageControl = parentControl as Panel;

            imageControl.ToolTip = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id) + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);

            if (matchedRemoteDoc.HasFocus)
            {
                imageControl.Children.OfType<Border>().Single().Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                imageControl.Children.OfType<Border>().Single().Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
