using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    public class SharedFolderLocalModelPersister : FileBasedLocalModelPersisterBase
    {
        protected override string PersistenceFolderPath => TeamCodingPackage.Current.Settings.FileBasedPersisterPath;
        public SharedFolderLocalModelPersister(LocalIDEModel model) : base(model)
        {
            TeamCodingPackage.Current.Settings.FileBasedPersisterPathChanged += Settings_FileBasedPersisterPathChanged;
        }

        private void Settings_FileBasedPersisterPathChanged(object sender, EventArgs e)
        {
            // TODO: clean up any existing file
            SendChanges();
        }
    }
}
