using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.DebugPersister
{
    public class DebugRemoteModelPersister : FileBasedRemoteModelPersisterBase
    {
        protected override string PersistenceFolderPath => Directory.GetCurrentDirectory();

        public DebugRemoteModelPersister() : base() { }
    }
}
