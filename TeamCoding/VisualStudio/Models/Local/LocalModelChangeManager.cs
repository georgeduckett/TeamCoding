using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.SourceControl;
using TeamCoding.VisualStudio;

namespace TeamCoding.VisualStudio.Models.Local
{
    /// <summary>
    /// Handles sending local IDE model changes to other clients.
    /// </summary>
    public class LocalModelChangeManager : IDisposable
    {
        private readonly string PersistenceFileSearchFormat = $"OpenDocs{Environment.MachineName}_*.bin";
        private readonly string PersistenceFile = $"OpenDocs{Environment.MachineName}_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
        private readonly LocalIDEModel IdeModel;

        public LocalModelChangeManager(LocalIDEModel model)
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
            
            // Create a remote IDE model to send
            var remoteModel = new Remote.RemoteIDEModel(IdeModel);

            using (var f = File.Create(PersistenceFile))
            {
                ProtoBuf.Serializer.Serialize(f, remoteModel);
            }
        }

        public void Dispose()
        {
            if (File.Exists(PersistenceFile))
            {
                File.Delete(PersistenceFile);
            }
        }
    }
}
