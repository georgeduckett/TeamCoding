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
        private readonly string PersistenceFile = $"OpenDocs{Environment.MachineName}_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
        private readonly LocalIDEModel IdeModel;

        public ModelChangeManager(LocalIDEModel model)
        {
            IdeModel = model;
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;
        }

        private void IdeModel_TextDocumentSaved(object sender, Microsoft.VisualStudio.Text.TextDocumentFileActionEventArgs e)
        {
            SendChanges();
        }

        private void IdeModel_TextContentChanged(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            // TODO: Handle IdeModel_TextContentChanged to sync changes between instances (if enabled in some setting somehow?), maybe also to allow quick notifications of editing a document
            // SendChanges();
        }
        private void IdeModel_OpenViewsChanged(object sender, EventArgs e)
        {
            SendChanges();
        }

        private void SendChanges()
        {
            // TODO: Persist somewhere other than a file! (maybe UDP broadcast to local network for now, (or write to a file share?)
            var newItems = IdeModel.OpenDocs();
            if (File.Exists(PersistenceFile))
            {
                File.Delete(PersistenceFile);
            }
            // TODO: Somewhere we need to clean-up old files from crashed IDEs - possibly we use a heartbeat method and delete old files order than that
            // Create a remote IDE model to send
            var remoteModel = new RemoteIDEModel(IdeModel);

            if (newItems.Length != 0)
            {
                using (var f = File.Create(PersistenceFile))
                {
                    ProtoBuf.Serializer.Serialize(f, remoteModel);
                }
            }
        }
    }
}
