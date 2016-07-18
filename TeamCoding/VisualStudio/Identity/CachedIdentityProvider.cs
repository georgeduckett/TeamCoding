using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    public class CachedIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity Identity;
        public CachedIdentityProvider(IIdentityProvider identityProvider)
        {
            Identity = identityProvider.GetIdentity();
        }
        public UserIdentity GetIdentity() => Identity;
    }
}
