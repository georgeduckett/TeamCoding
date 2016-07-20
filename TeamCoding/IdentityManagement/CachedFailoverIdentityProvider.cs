using System;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// An identity provider that tries child identity providers in turn until we get a valid identity
    /// </summary>
    public class CachedFailoverIdentityProvider : IIdentityProvider
    {
        private UserIdentity Identity;
        private readonly IIdentityProvider[] IdentityProviders;
        private bool _ShouldCache;
        public bool ShouldCache => _ShouldCache;
        public UserIdentity GetIdentity()
        {
            if (!_ShouldCache)
            {
                SetIdentity();
            }
            return Identity;
        }
        public CachedFailoverIdentityProvider(params IIdentityProvider[] identityProviders)
        {
            IdentityProviders = identityProviders;
            SetIdentity();
        }
        private void SetIdentity()
        {
            _ShouldCache = true;
            foreach (var identityProvider in IdentityProviders)
            {
                Identity = identityProvider.GetIdentity();
                if (Identity != null)
                {
                    if (!identityProvider.ShouldCache)
                    {
                        // Once we get a valid identity, if the provider we got it from shouldn't be cached then don't.
                        _ShouldCache = false;
                    }
                    break;
                }
            }
        }
    }
}
