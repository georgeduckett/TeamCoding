using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    public class VSIdentityProvider : IIdentityProvider
    {
        private const string SubKey = "Software\\Microsoft\\VSCommon\\ConnectedUser\\IdeUser\\Cache";
        private const string EmailAddressKeyName = "EmailAddress";
        private const string UserNameKeyName = "DisplayName";
        private const string ImageKeyname = "Avatar.Small";
        private readonly UserIdentity Identity;

        public VSIdentityProvider()
        {
            try
            {
                RegistryKey root = Registry.CurrentUser;
                using (var sk = root.OpenSubKey(SubKey))
                {
                    if (sk != null)
                    {
                        var email = (string)sk.GetValue(EmailAddressKeyName);
                        Identity = new UserIdentity()
                        {
                            Id = (string)sk.GetValue(UserNameKeyName),
                            ImageUrl = UserIdentity.GetGravatarUrlFromEmail(email),
                            ImageBytes = ((byte[])sk.GetValue(ImageKeyname)).Skip(16).ToArray() // TODO: Detect if this is a simple initials image and don't use it if that's the case (it displays badly when small), maybe generate one client side instead
                        };
                    }
                }
            }
            catch
            {
                Identity = null;
            }
        }

        public UserIdentity GetIdentity() => Identity;
    }
}
