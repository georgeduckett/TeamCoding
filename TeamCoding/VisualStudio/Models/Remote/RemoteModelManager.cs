using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.Remote
{
    /// <summary>
    /// Manages updating local models of remote IDEs
    /// </summary>
    public class RemoteModelManager
    {
        private const string ModelSyncFileFormat = "OpenDocs*.bin";
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
                while (!RemoteModelChangeManager.IsFileReady(modelSyncFile)) { }
                using (var f = File.OpenRead(modelSyncFile))
                {
                    Models.Add(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                }
            }
        }
    }
}