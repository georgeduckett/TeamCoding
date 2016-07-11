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
        private string PersistenceFile = $"OpenDocs{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
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

            // Create a remote IDE model to send
            // TODO: Make a better constructor for RemoteIDEModel and use it
            var remoteModel = new RemoteIDEModel(Enumerable.Repeat(new MachineIdentityProvider().GetIdentity(), 1).Union(NewItems.Select(i => i.BeingEdited.ToString() + " " + i.RelativePath)).ToArray());

            if (NewItems.Length != 0)
            {
                using (var f = File.Create(PersistenceFile))
                {
                    ProtoBuf.Serializer.Serialize(f, remoteModel);
                }
            }
        }
    }
}
