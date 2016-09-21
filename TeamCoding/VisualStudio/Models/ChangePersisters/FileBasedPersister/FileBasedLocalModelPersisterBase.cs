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
    public abstract class FileBasedLocalModelPersisterBase : LocalModelPersisterBase
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
            :base(model, TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPathProperty)
        {
            FileHeartBeatCancelSource = new CancellationTokenSource();
            FileHeartBeatCancelToken = FileHeartBeatCancelSource.Token;
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
        protected override void SendModel(RemoteIDEModel remoteModel)
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

        public override void Dispose()
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
