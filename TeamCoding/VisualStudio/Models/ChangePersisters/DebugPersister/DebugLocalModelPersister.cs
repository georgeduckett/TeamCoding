using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.DebugPersister
{
    public class DebugLocalModelPersister : FileBasedLocalModelPersisterBase
    {
        protected override string PersistenceFolderPath => Directory.GetCurrentDirectory();

        public DebugLocalModelPersister(LocalIDEModel model) : base(model) { }
        protected override void SendModel(RemoteIDEModel model)
        {
            if (PersistenceFolderPath != null && Directory.Exists(PersistenceFolderPath))
            {
                // Delete any temporary persistence files
                foreach (var file in Directory.EnumerateFiles(PersistenceFolderPath, PersistenceFileSearchFormat))
                {
                    if (File.Exists(file) && file != PersistenceFilePath)
                    {
                        File.Delete(file);
                    }
                }
            }
            base.SendModel(model);
        }
    }
}
