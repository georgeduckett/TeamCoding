using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    /// <summary>
    /// Handles sending local IDE model changes to other clients.
    /// Used for debugging. Writes the local model to the current directory, using protobuf.
    /// </summary>
    public abstract class FileBasedLocalModelPersisterBase : ILocalModelPerisister
    {
        private static readonly string SessionId = Guid.NewGuid().ToString("N");
        protected readonly string PersistenceFileSearchFormat = $"OpenDocs{SessionId}_*.bin";
        protected readonly string PersistenceFile = $"OpenDocs{SessionId}_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin";
        private readonly LocalIDEModel IdeModel;
        protected abstract string PersistenceFolderPath { get; }
        protected string PersistenceFilePath => PersistenceFolderPath == null ? null : Path.Combine(PersistenceFolderPath, PersistenceFile);

        private DateTime LastFileWriteTime = DateTime.UtcNow;
        private readonly Thread FileHeartBeatThread;
        private CancellationTokenSource FileHeartBeatCancelSource;
        private CancellationToken FileHeartBeatCancelToken;
        public FileBasedLocalModelPersisterBase(LocalIDEModel model)
        {
            TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPathChanging += SharedSettings_FileBasedPersisterPathChanging;
            TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPathChanged += Settings_FileBasedPersisterPathChanged;

            FileHeartBeatCancelSource = new CancellationTokenSource();
            FileHeartBeatCancelToken = FileHeartBeatCancelSource.Token;
            IdeModel = model;
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;

            FileHeartBeatThread = new Thread(() =>
            {
                while (!FileHeartBeatCancelToken.IsCancellationRequested)
                {
                    if (PersistenceFolderPath == null || !Directory.Exists(PersistenceFolderPath))
                    {
                        FileHeartBeatCancelToken.WaitHandle.WaitOne(5000);
                    }
                    else
                    {
                        try
                        {
                            var UTCNow = DateTime.UtcNow;
                            var Difference = (UTCNow - LastFileWriteTime).TotalSeconds;
                            if (Difference > 60)
                            { // If there have been no changes in the last minute, write the file again (prevent it being tidied up by others)
                                File.SetLastWriteTimeUtc(PersistenceFilePath, UTCNow);
                                LastFileWriteTime = UTCNow;
                                FileHeartBeatCancelToken.WaitHandle.WaitOne(1000 * 60);
                            }
                            else
                            {
                                FileHeartBeatCancelToken.WaitHandle.WaitOne(1000 * (60 - (int)Difference + 1));
                            }
                        }
                        catch (IOException)
                        {
                            FileHeartBeatCancelToken.WaitHandle.WaitOne(10000);
                        }
                    }
                }
            });

            FileHeartBeatThread.Start();
        }
        private void SharedSettings_FileBasedPersisterPathChanging(object sender, EventArgs e)
        {
            SendEmpty();
        }
        private void Settings_FileBasedPersisterPathChanged(object sender, EventArgs e)
        {
            SendChanges();
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
        protected virtual void SendEmpty()
        {
            SendIdeModel(new RemoteIDEModel(new LocalIDEModel()));
        }
        protected virtual void SendChanges()
        {
            SendIdeModel(new RemoteIDEModel(IdeModel));
        }

        private void SendIdeModel(RemoteIDEModel remoteModel)
        {
            if (!string.IsNullOrEmpty(PersistenceFolderPath))
            {
                try
                {
                    using (var f = File.Create(PersistenceFilePath))
                    {
                        ProtoBuf.Serializer.Serialize(f, remoteModel);
                        LastFileWriteTime = DateTime.UtcNow;
                    }
                }
                catch (IOException ex)
                {
                    TeamCodingPackage.Current.Logger.WriteError("Failed to create persistence file.");
                    TeamCodingPackage.Current.Logger.WriteError(ex);
                }
            }
        }

        public void Dispose()
        {
            FileHeartBeatCancelSource.Cancel();
            FileHeartBeatThread.Join();
            if (File.Exists(PersistenceFilePath))
            {
                File.Delete(PersistenceFilePath);
            }
        }
    }
}
