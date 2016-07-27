using Microsoft.Win32;
using System;
using System.Drawing;
using System.Linq;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// Gets a user identity from the visual studio registry
    /// </summary>
    public class VSIdentityProvider : IIdentityProvider
    {
        private const string SubKey = "Software\\Microsoft\\VSCommon\\ConnectedUser\\IdeUser\\Cache";
        private const string EmailAddressKeyName = "EmailAddress";
        private const string UserNameKeyName = "DisplayName";
        private const string ImageKeyname = "Avatar.Small";
        private readonly UserIdentity Identity;
        public bool ShouldCache => true;
        public VSIdentityProvider()
        {
            try
            {
                RegistryKey root = Registry.CurrentUser;
                using (var sk = root.OpenSubKey(SubKey))
                {
                    if (sk != null)
                    {
                        var userName = (string)sk.GetValue(UserNameKeyName);
                        var email = (string)sk.GetValue(EmailAddressKeyName);
                        var bytes = ((byte[])sk.GetValue(ImageKeyname)).Skip(16).ToArray();
                        Identity = new UserIdentity()
                        {
                            Id = userName,
                            DisplayName = userName,
                            ImageUrl = UserIdentity.GetGravatarUrlFromEmail(email),
                            ImageBytes = IsVSPlaceholderImage(bytes) ? null : bytes, // Only use the image if it's not a default one
                        };
                    }
                }
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                TeamCodingPackage.Current.Logger.WriteError(ex);
                Identity = null;
            }
        }
        /// <summary>
        /// Indicates whether the image defined by the given byte array is likely to be a placeolder one generate by visual studio based on the user's initials
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        private bool IsVSPlaceholderImage(byte[] imageBytes)
        {
            var image = Image.FromStream(new System.IO.MemoryStream(imageBytes)) as Bitmap;

            var Colour = image.GetPixel(0, 0);

            if (Colour == image.GetPixel(1, 1) &&
                Colour == image.GetPixel(0, image.Height - 1) &&
                Colour == image.GetPixel(1, image.Height - 2) &&
                Colour == image.GetPixel(image.Width - 1, 0) &&
                Colour == image.GetPixel(image.Width - 2, 1) &&
                Colour == image.GetPixel(image.Width - 1, image.Height - 1) &&
                Colour == image.GetPixel(image.Width - 2, image.Height - 2))
            {
                // Corner pixels, plus one pixel in from the corner are the same colour  
                return true;
            }

            return false;
        }

        public UserIdentity GetIdentity() => Identity;
    }
}
