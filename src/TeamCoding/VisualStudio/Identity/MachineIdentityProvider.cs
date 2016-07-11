using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Identity
{
    public class MachineIdentityProvider : IIdentityProvider
    {
        public string GetIdentity()
        {
            return Environment.UserName;
        }
    }
}
