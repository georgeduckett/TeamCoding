using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    public class SharedFolderLocalModelPersister : FileBasedLocalModelPersisterBase
    {
        protected override string PersistenceFolderPath => TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPath;
        public SharedFolderLocalModelPersister(LocalIDEModel model) : base(model)
        {
        }
    }
}
