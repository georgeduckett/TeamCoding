using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    public class VSUIIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity _Identity;

        public VSUIIdentityProvider()
        {
            _Identity = null;
            throw new NotImplementedException();
        }

        public UserIdentity GetIdentity() => _Identity;
    }
}
