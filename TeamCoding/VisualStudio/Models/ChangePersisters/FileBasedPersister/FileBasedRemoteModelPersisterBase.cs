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
    public abstract class FileBasedRemoteModelPersisterBase : RemoteModelPersisterBase
    {
        public const string ModelSyncFileFormat = "OpenDocs*.bin";
        protected abstract string PersistenceFolderPath { get; }
        private FileSystemWatcher FileWatcher;
        public FileBasedRemoteModelPersisterBase()
        {
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
        }
        private void SyncChanges()
        {
            ClearRemoteModels();
            foreach (var modelSyncFile in Directory.GetFiles(PersistenceFolderPath, ModelSyncFileFormat))
            {
                while (!IsFileReady(modelSyncFile)) { }
                // If any file hasn't been modified in the last minute an a half, delete it (tidy up files left from crashes etc.)
                if((DateTime.UtcNow - File.GetLastWriteTimeUtc(modelSyncFile)).TotalSeconds > 90)
                {
                    File.Delete(modelSyncFile);
                    continue;
                }
                using (var f = File.OpenRead(modelSyncFile))
                {
                    OnRemoteModelReceived(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();
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
