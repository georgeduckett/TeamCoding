using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.DebugPersister
{
    /// <summary>
    /// Manages receiving remote IDE model changes
    /// </summary>
    public class DebugRemoteModelPersister : IDisposable
    {
        private readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), "*.bin");
        private readonly IDEWrapper IDEWrapper;
        private readonly List<RemoteIDEModel> RemoteModels = new List<RemoteIDEModel>();
        public IEnumerable<SourceControlledDocumentData> GetOpenFiles() => RemoteModels.SelectMany(model => model.OpenFiles.Select(of => new SourceControlledDocumentData()
        {
            Repository = of.RepoUrl,
            IdeUserIdentity = model.IDEUserIdentity,
            RelativePath = of.RelativePath,
            BeingEdited = of.BeingEdited,
            HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault()
        }));
        public DebugRemoteModelPersister(IDEWrapper ideWrapper)
        {
            IDEWrapper = ideWrapper;
            FileWatcher.Created += FileWatcher_Changed;
            FileWatcher.Deleted += FileWatcher_Changed;
            FileWatcher.Changed += FileWatcher_Changed;
            FileWatcher.Renamed += FileWatcher_Changed;
            FileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            FileWatcher.EnableRaisingEvents = true;
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileWatcher.EnableRaisingEvents = false;
            if(e.ChangeType != WatcherChangeTypes.Deleted)
            {
                while (!IsFileReady(e.FullPath)) { }
            }
            FileWatcher.EnableRaisingEvents = true;
            SyncChanges();
            IDEWrapper.UpdateIDE();
        }
        private void SyncChanges()
        {
            RemoteModels.Clear();
            foreach (var modelSyncFile in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), DebugLocalModelPersister.ModelSyncFileFormat))
            {
                while (!IsFileReady(modelSyncFile)) { }
                using (var f = File.OpenRead(modelSyncFile))
                {
                    RemoteModels.Add(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                }
            }
        }
        public void Dispose()
        {
            FileWatcher.Dispose();
        }
        public static bool IsFileReady(string sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;

                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
