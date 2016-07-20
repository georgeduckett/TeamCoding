using System;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// Gets the machine names as the identity
    /// </summary>
    public class MachineIdentityProvider : IIdentityProvider
    {
        public bool ShouldCache => true;
        public UserIdentity GetIdentity()
        {
            return new UserIdentity() { Id = Environment.UserName, DisplayName = Environment.UserName };
        }
    }
}
