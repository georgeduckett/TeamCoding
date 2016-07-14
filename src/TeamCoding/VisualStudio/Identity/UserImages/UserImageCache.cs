using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeamCoding.SourceControl;

namespace TeamCoding.VisualStudio.Identity.UserImages
{
    public class UserImageCache
    {
        private static readonly Brush BorderBrush = Brushes.White;
        private readonly Border SharedUnknownUserImage = new Border() { BorderBrush = BorderBrush, Child = new Image() { Source = LoadBitmapFromResource("Resources/UnknownUserImage.png") } };
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();

        public Border GetUserImageFromUrl(string url)
        {
            if (url == null) { return new Border() { BorderBrush = BorderBrush, Child = new Image() { Source = (SharedUnknownUserImage.Child as Image).Source } }; }

            if (UrlImages.ContainsKey(url))
            {
                return new Border() { BorderBrush = BorderBrush, Child = new Image() { Source = UrlImages[url] } };
            }

            var result = new Border() { BorderBrush = BorderBrush, Child = new Image() { Source = (SharedUnknownUserImage.Child as Image).Source } };

            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(() =>
            {
                using (MemoryStream stream = new MemoryStream(new System.Net.WebClient().DownloadData(url)))
                {
                    (result.Child as Image).Source = UrlImages[url] = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
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
        internal void SetImageProperties(Border image, RemoteDocumentData matchedRemoteDoc)
        {
            image.ToolTip = matchedRemoteDoc.IdeUserIdentity.DisplayName + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);
            // TODO: Give the image a border or not
            if (matchedRemoteDoc.HasFocus)
            {
                image.BorderThickness = new System.Windows.Thickness(1.0);
            }
            else
            {
                image.BorderThickness = new System.Windows.Thickness(0.0);
            }
        }
    }
}
