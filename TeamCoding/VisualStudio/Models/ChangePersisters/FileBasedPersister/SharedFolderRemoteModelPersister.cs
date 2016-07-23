using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    public class SharedFolderRemoteModelPersister : FileBasedRemoteModelPersisterBase
    {
        protected override string PersistenceFolderPath => TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPath;
        public SharedFolderRemoteModelPersister() : base() { }
    }
}
