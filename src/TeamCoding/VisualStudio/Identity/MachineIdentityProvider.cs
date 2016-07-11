using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    public class MachineIdentityProvider : IIdentityProvider
    {
        public UserIdentity GetIdentity()
        {
            return new UserIdentity() { DisplayName = Environment.UserName };
        }
    }
}
