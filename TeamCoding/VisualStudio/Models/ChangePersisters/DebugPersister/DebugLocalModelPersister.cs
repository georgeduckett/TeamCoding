using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.DebugPersister
{
    /// <summary>
    /// Handles sending local IDE model changes to other clients.
    /// </summary>
    public class DebugLocalModelPersister : IDisposable
    {
        public const string ModelSyncFileFormat = "OpenDocs*.bin";
        private readonly List<RemoteIDEModel> Models = new List<RemoteIDEModel>();
        public IEnumerable<SourceControlledDocumentData> GetOpenFiles() => Models.SelectMany(model => model.OpenFiles.Select(of => new SourceControlledDocumentData()
        {
            Repository = of.RepoUrl,
            IdeUserIdentity = model.IDEUserIdentity,
            RelativePath = of.RelativePath,
            BeingEdited = of.BeingEdited,
            HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault()
        }));

        public void SyncChanges()
        {
            Models.Clear();
            foreach (var modelSyncFile in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), ModelSyncFileFormat))
            {
                while (!DebugRemoteModelPersister.IsFileReady(modelSyncFile)) { }
                using (var f = File.OpenRead(modelSyncFile))
                {
                    Models.Add(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                }
            }
        }
        private readonly string PersistenceFileSearchFormat = $"OpenDocs{Environment.MachineName}_*.bin";
        private readonly string PersistenceFile = $"OpenDocs{Environment.MachineName}_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
        private readonly LocalIDEModel IdeModel;

        public DebugLocalModelPersister(LocalIDEModel model)
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
            // Delete any temporary persistence files
            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), PersistenceFileSearchFormat))
            {
                if (File.Exists(file) && file != PersistenceFile)
                {
                    File.Delete(file);
                }
            }
            // Create a remote IDE model to send
            var remoteModel = new RemoteIDEModel(IdeModel);

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
