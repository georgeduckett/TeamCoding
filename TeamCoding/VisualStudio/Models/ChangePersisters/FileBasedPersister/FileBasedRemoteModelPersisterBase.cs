using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    /// <summary>
    /// Manages receiving remote IDE model changes.
    /// Used for debugging. Reads from the current directory, using protobuf.
    /// </summary>
    public abstract class FileBasedRemoteModelPersisterBase : IRemoteModelPersister
    {
        public const string ModelSyncFileFormat = "OpenDocs*.bin";
        protected abstract string PersistenceFolderPath { get; }
        private FileSystemWatcher FileWatcher;
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
        public FileBasedRemoteModelPersisterBase(IDEWrapper ideWrapper)
        {
            IDEWrapper = ideWrapper;
            TeamCodingPackage.Current.Settings.FileBasedPersisterPathChanged += Settings_FileBasedPersisterPathChanged;

            if (PersistenceFolderPath != null && Directory.Exists(PersistenceFolderPath))
            {
                FileWatcher = new FileSystemWatcher(PersistenceFolderPath, "*.bin");
                FileWatcher.Created += FileWatcher_Changed;
                FileWatcher.Deleted += FileWatcher_Changed;
                FileWatcher.Changed += FileWatcher_Changed;
                FileWatcher.Renamed += FileWatcher_Changed;
                FileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
                FileWatcher.EnableRaisingEvents = true;
            }
        }

        private void Settings_FileBasedPersisterPathChanged(object sender, EventArgs e)
        {
            FileWatcher?.Dispose();
            if (PersistenceFolderPath != null && Directory.Exists(PersistenceFolderPath))
            {
                FileWatcher = new FileSystemWatcher(PersistenceFolderPath, "*.bin");
                FileWatcher.Created += FileWatcher_Changed;
                FileWatcher.Deleted += FileWatcher_Changed;
                FileWatcher.Changed += FileWatcher_Changed;
                FileWatcher.Renamed += FileWatcher_Changed;
                FileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
                FileWatcher.EnableRaisingEvents = true;
                // Sync any changes since there could already be files in this new directory waiting
                SyncChanges();
            }
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
            foreach (var modelSyncFile in Directory.EnumerateFiles(PersistenceFolderPath, ModelSyncFileFormat))
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
            FileWatcher?.Dispose();
        }
        private bool IsFileReady(string sFilename)
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
