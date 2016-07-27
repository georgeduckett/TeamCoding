using System;
using System.Linq;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// An identity provider that tries child identity providers in turn until we get a valid identity
    /// </summary>
    public class CachedFailoverIdentityProvider : IIdentityProvider
    {
        private UserIdentity Identity;
        private readonly IIdentityProvider[] IdentityProviders;
        public bool ShouldCache => IdentityProviders.All(ip => ip.ShouldCache);
        public UserIdentity GetIdentity()
        {
            foreach (var identityProvider in IdentityProviders)
            {
                if (identityProvider.ShouldCache && Identity != null)
                {
                    return Identity;
                }
                else
                {
                    var newIdentity = identityProvider.GetIdentity();

                    if(newIdentity != null)
                    {
                        Identity = newIdentity;
                        TeamCodingPackage.Current.Logger.WriteInformation($"{nameof(CachedFailoverIdentityProvider)}: Got identity from {identityProvider.GetType().Name}");
                        return Identity;
                    }
                }
            }

            throw new InvalidOperationException("Failed to get a user identity after trying all providers: " + string.Join(", ", IdentityProviders.Select(ip => ip.GetType().Name)));
        }
        public CachedFailoverIdentityProvider(params IIdentityProvider[] identityProviders)
        {
            TeamCodingPackage.Current.Logger.WriteInformation(
                $"{nameof(CachedFailoverIdentityProvider)}: Using identity providers; {string.Join(", ", identityProviders.Select(ip => ip.GetType().Name))}");

            IdentityProviders = identityProviders;
        }
    }
}
