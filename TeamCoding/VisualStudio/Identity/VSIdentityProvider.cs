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
        private readonly UserIdentity Identity;

        public VSIdentityProvider()
        {
            RegistryKey root = Registry.CurrentUser;
            using (var sk = root.OpenSubKey(SubKey))
            {
                if (sk != null)
                {
                    var email = (string)sk.GetValue(EmailAddressKeyName);
                    Identity = new UserIdentity()
                    { // TODO: Maybe figure out a way of including the actual image in visual studio (available in the registry) without having to send it every time for the VSIdentityProvider
                        Id = (string)sk.GetValue(UserNameKeyName),
                        ImageUrl = UserIdentity.GetGravatarUrlFromEmail(email)
                    };

                    TeamCodingPackage.Current.IDEWrapper.InvokeAsync(async () =>
                    {
                        Identity.Id = await UserIdentity.GetGravatarDisplayNameFromEmail(email);
                        TeamCodingPackage.Current.LocalIdeModel.OnUserIdentityChanged();
                    });
                }
            }
        }

        public UserIdentity GetIdentity() => Identity;
    }
}
