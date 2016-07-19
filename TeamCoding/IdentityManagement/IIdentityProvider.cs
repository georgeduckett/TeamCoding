using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.IdentityManagement
{
    public interface IIdentityProvider
    {
        UserIdentity GetIdentity();
    }
}
