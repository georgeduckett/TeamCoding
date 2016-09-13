using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Interfaces.Documents;
using TeamCoding.Logging;
using TeamCoding.VisualStudio.Models.ChangePersisters;

namespace TeamCoding
{
    [Export(typeof(ITeamCodingPackageProvider))]
    public class TeamCodingPackageProvider : ITeamCodingPackageProvider
    {
        public ICaretAdornmentDataProvider CaretAdornmentDataProvider => TeamCodingPackage.Current.CaretAdornmentDataProvider;
        public ICaretInfoProvider CaretInfoProvider => TeamCodingPackage.Current.CaretInfoProvider;
        public HttpClient HttpClient => TeamCodingPackage.Current.HttpClient;
        public IRemoteModelPersister RemoteModelChangeManager => TeamCodingPackage.Current.RemoteModelChangeManager;
        public ILogger Logger => TeamCodingPackage.Current.Logger;
    }
}
