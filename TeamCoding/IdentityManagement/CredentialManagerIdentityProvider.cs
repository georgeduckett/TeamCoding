using TeamCoding.CredentialManagement;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// Gets crendials from the windows credential manager for a given set of credential targets (tried in turn)
    /// </summary>
    public class CredentialManagerIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity Identity;
        public UserIdentity GetIdentity() => Identity;
        public CredentialManagerIdentityProvider(string[] credentialTargets)
        {
            Credential credential = null;
            foreach (var credentialTarget in credentialTargets)
            {
                credential = new Credential { Target = credentialTarget };
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
