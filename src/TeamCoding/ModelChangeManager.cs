using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio;

namespace TeamCoding
{
    /// <summary>
    /// Handles persisting changes to the IDEModel. For now just persist to disk as a test
    /// </summary>
    internal class ModelChangeManager
    {
        private const string PersistenceFile = "OpenDocs.txt";
        private readonly IDEModel _IdeModel;

        public ModelChangeManager(IDEModel model)
        {
            _IdeModel = model;
            _IdeModel.Changed += IdeModel_Changed;
        }

        private void IdeModel_Changed(object sender, EventArgs e)
        {
            var NewItems = _IdeModel.OpenDocs();
            if (File.Exists(PersistenceFile))
            {
                File.Delete(PersistenceFile);
            }

            File.WriteAllLines(PersistenceFile, NewItems.Select(i => i.BeingEdited.ToString() + " " + i.RelativePath));
            var test = Environment.UserName;
        }
    }
}
