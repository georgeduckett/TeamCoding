namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// An identity provider that tries child identity providers in turn until we get a valid identity
    /// </summary>
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
