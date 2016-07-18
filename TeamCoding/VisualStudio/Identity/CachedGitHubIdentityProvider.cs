using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.CredentialManagement;

namespace TeamCoding.VisualStudio.Identity
{
    public class CachedGitHubIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity Identity;
        public UserIdentity GetIdentity() => Identity;
        public CachedGitHubIdentityProvider()
        {
            Credential credential = new Credential { Target = "git:https://github.com" };
            credential.Load();

            if (credential.Username == null)
            {
                credential = new Credential { Target = "https://github.com" };
                credential.Load();
            }

            Identity = new UserIdentity()
            {
                Id = credential.Username,
                ImageUrl = UserIdentity.GetGravatarUrlFromEmail(credential.Username)
            };

            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(async () =>
            {
                Identity.Id = await UserIdentity.GetGravatarDisplayNameFromEmail(credential.Username);
                TeamCodingPackage.Current.LocalIdeModel.OnUserIdentityChanged();
            });
        }

    }
}
