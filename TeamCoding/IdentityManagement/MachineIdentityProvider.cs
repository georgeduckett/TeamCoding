using System;
using TeamCoding.VisualStudio.Models;

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
            return new UserIdentity() { Id = LocalIDEModel.Id.Value, DisplayName = Environment.UserName };
        }
    }
}
