using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Identity
{
    public class VSIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity _Identity;

        public VSIdentityProvider()
        {
            // TODO: Flesh out the VSIdentityProvider class using SVsConnectedUserService (undocumented?)
            //IVsConnectedIdeUserContext vsConnectedIdeUserContext = Package.GetGlobalService(typeof(SVsConnectedUserService)) as IVsConnectedIdeUserContext;
            // Microsoft.VisualStudio.Shell.Connected.dll (in GAC)
            _Identity = null;
            throw new NotImplementedException();
        }

        public UserIdentity GetIdentity() => _Identity;
    }
}
