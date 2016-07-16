using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TeamCoding.Models;

namespace TeamCoding.VisualStudio.Models
{
    public class RemoteModelChangeManager : IDisposable
    {
        private readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), "*.bin");
        private readonly IDEWrapper IDEWrapper;
        private readonly ExternalModelManager RemoteModelManager;
        public RemoteModelChangeManager(IDEWrapper ideWrapper, ExternalModelManager remoteModelManager)
        {
            RemoteModelManager = remoteModelManager;
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
            RemoteModelManager.SyncChanges();
            IDEWrapper.UpdateIDE();
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
