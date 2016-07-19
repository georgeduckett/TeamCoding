namespace TeamCoding.IdentityManagement
{
    public class CachedFailoverIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity Identity;
        public UserIdentity GetIdentity() => Identity;
        public CachedFailoverIdentityProvider(params IIdentityProvider[] identityProviders)
        {
            foreach(var identityProvider in identityProviders)
            {
                Identity = identityProvider.GetIdentity();
                if(Identity != null)
                {
                    break;
                }
            }
        }
    }
}
