using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio.Identity;

namespace TeamCoding.VisualStudio
{
    public class UserImageCache
    { // TODO: See how github tools for visual studio gets/caches user images
        private static readonly Brush BorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        private readonly BitmapImage SharedUnknownUserImage = LoadBitmapFromResource("Resources/UnknownUserImage.png");
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();
        private readonly IDEWrapper IdeWrapper;

        public UserImageCache(IDEWrapper ideWrapper)
        {
            IdeWrapper = ideWrapper;
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

        public Panel GetUserImageControlFromUserIdentity(UserIdentity userIdentity)
        {
            Panel result;
            if (userIdentity.ImageBytes == null)
            {
                var url = userIdentity.ImageUrl;
                if (url == null) { return CreateUserImageControl(SharedUnknownUserImage); }

                if (UrlImages.ContainsKey(url))
                {
                    return CreateUserImageControl(UrlImages[url]);
                }

                result = CreateUserImageControl(SharedUnknownUserImage);

                IdeWrapper.InvokeAsync(async () =>
                {
                    try
                    {
                        var request = await TeamCodingPackage.Current.HttpClient.GetAsync(url);
                        if (!request.IsSuccessStatusCode) return;
                        var imageStream = await request.Content.ReadAsStreamAsync();
                        result.Children.OfType<Image>().Single().Source = UrlImages[url] = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    catch { } // Any failures don't matter, it just won't update the image
                });
            }
            else
            {
                // TODO: Maybe don't use the visual studio user image, as when it's just initials it's impossible to read. We also don't really want it sent across the wire every time either
                using (var MS = new MemoryStream(userIdentity.ImageBytes.Skip(16).ToArray()))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = MS;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit(); // TODO: Why doesn't the visual studio Avatar.Small load correctly?
                    result = CreateUserImageControl(bitmap);
                }
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
        internal void SetImageProperties(Panel parentControl, RemoteDocumentData matchedRemoteDoc)
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
