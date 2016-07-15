using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeamCoding.SourceControl;

namespace TeamCoding.VisualStudio.Identity.UserImages
{
    public class UserImageCache
    {
        private static readonly Brush BorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        private readonly BitmapImage SharedUnknownUserImage = LoadBitmapFromResource("Resources/UnknownUserImage.png");
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();

        private static Grid CreateGrid(ImageSource imageSource)
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

        public Grid GetUserImageFromUrl(string url)
        {
            if (url == null) { return CreateGrid(SharedUnknownUserImage); }

            if (UrlImages.ContainsKey(url))
            {
                return CreateGrid(UrlImages[url]);
            }

            var result = CreateGrid(SharedUnknownUserImage);

            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(() =>
            {
                using (MemoryStream stream = new MemoryStream(new System.Net.WebClient().DownloadData(url)))
                {
                    result.Children.OfType<Image>().Single().Source = UrlImages[url] = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            });

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
        internal void SetImageProperties(FrameworkElement parentControl, RemoteDocumentData matchedRemoteDoc)
        {
            var imageControl = parentControl as Panel;

            imageControl.ToolTip = matchedRemoteDoc.IdeUserIdentity.DisplayName + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);

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
