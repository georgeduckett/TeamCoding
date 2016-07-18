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
    public class CredentialManagerGitHubIdentityProvider : IIdentityProvider
    {
        private static string[] GitHubCredentialTargets = new[] { "git:https://github.com", "https://github.com" };

        private readonly UserIdentity Identity;
        public UserIdentity GetIdentity() => Identity;
        public CredentialManagerGitHubIdentityProvider()
        {
            Credential credential = null;
            foreach (var gitHubCredentialTarget in GitHubCredentialTargets)
            {
                credential = new Credential { Target = gitHubCredentialTarget };
                if(credential.Load() && credential.Username != null)
                {
                    break;
                }
            }
            
            if(credential?.Username == null)
            {
                Identity = null;
                return;
            }

            Identity = new UserIdentity()
            {
                Id = credential.Username,
                DisplayName = credential.Username,
                ImageUrl = UserIdentity.GetGravatarUrlFromEmail(credential.Username)
            };

            TeamCodingPackage.Current.IDEWrapper.InvokeAsync(async () =>
            {
                Identity.DisplayName = await UserIdentity.GetGravatarDisplayNameFromEmail(credential.Username);
                TeamCodingPackage.Current.LocalIdeModel.OnUserIdentityChanged();
            });
        }

    }
}
