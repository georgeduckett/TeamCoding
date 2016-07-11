using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.SourceControl
{
    public class VSUIIdentityProvider : IIdentityProvider
    {
        private readonly string _Identity;

        public VSUIIdentityProvider()
        {
            _Identity = null;
            throw new NotImplementedException();
        }

        public string GetIdentity() => _Identity;
    }
}
