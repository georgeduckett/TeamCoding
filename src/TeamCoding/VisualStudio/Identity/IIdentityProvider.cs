using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Identity
{
    public interface IIdentityProvider
    {
        string GetIdentity();
    }
}
