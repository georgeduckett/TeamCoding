using System;

namespace TeamCoding.IdentityManagement
{
    public class MachineIdentityProvider : IIdentityProvider
    {
        public UserIdentity GetIdentity()
        {
            return new UserIdentity() { Id = Environment.UserName, DisplayName = Environment.UserName };
        }
    }
}
