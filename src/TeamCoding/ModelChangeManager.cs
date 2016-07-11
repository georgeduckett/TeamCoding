using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio;

namespace TeamCoding
{
    /// <summary>
    /// Handles persisting changes to the IDEModel. For now just persist to disk as a test
    /// </summary>
    internal class ModelChangeManager
    {
        private string PersistenceFile = $"OpenDocs{System.Diagnostics.Process.GetCurrentProcess().Id}.txt";
        private readonly LocalIDEModel _IdeModel;

        public ModelChangeManager(LocalIDEModel model)
        {
            _IdeModel = model;
            _IdeModel.Changed += IdeModel_Changed;
        }

        private void IdeModel_Changed(object sender, EventArgs e)
        {
            // TODO: Persist somewhere other than a file! (maybe UDP broadcast to local network for now)
            var NewItems = _IdeModel.OpenDocs();
            if (File.Exists(PersistenceFile))
            {
                File.Delete(PersistenceFile);
            }

            if (NewItems.Length != 0)
            {
                File.WriteAllLines(PersistenceFile, Enumerable.Repeat(new MachineIdentityProvider().GetIdentity(), 1).Union(NewItems.Select(i => i.BeingEdited.ToString() + " " + i.RelativePath)));
            }
        }
    }
}
