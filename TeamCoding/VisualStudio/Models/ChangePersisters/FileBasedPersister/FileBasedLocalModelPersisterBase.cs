using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    /// <summary>
    /// Handles sending local IDE model changes to other clients.
    /// Used for debugging. Writes the local model to the current directory, using protobuf.
    /// </summary>
    public abstract class FileBasedLocalModelPersisterBase : ILocalModelPerisister
    {
        protected readonly string PersistenceFileSearchFormat = $"OpenDocs{Environment.MachineName}_*.bin";
        protected readonly string PersistenceFile = $"OpenDocs{Environment.MachineName}_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
        private readonly LocalIDEModel IdeModel;
        protected abstract string PersistenceFolderPath { get; }
        protected string PersistenceFilePath => Path.Combine(PersistenceFolderPath, PersistenceFile);
        public FileBasedLocalModelPersisterBase(LocalIDEModel model)
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
        protected virtual void SendChanges()
        {
            // TODO: Handle IO errors
            if (PersistenceFolderPath != null && Directory.Exists(PersistenceFolderPath))
            {
                // Create a remote IDE model to send
                var remoteModel = new RemoteIDEModel(IdeModel);

                using (var f = File.Create(PersistenceFilePath))
                {
                    ProtoBuf.Serializer.Serialize(f, remoteModel);
                }
            }
        }
        public void Dispose()
        {
            if (File.Exists(PersistenceFilePath))
            {
                File.Delete(PersistenceFilePath);
            }
        }
    }
}
