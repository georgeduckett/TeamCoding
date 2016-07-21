using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisRemoteModelPersister : IRemoteModelPersister
    {
        public const string ModelPersisterChannel = "TeamCoding.ModelPersister";
        private static ConnectionMultiplexer RedisClient = ConnectionMultiplexer.Connect("localhost"); // TODO: allow for failing to connect to redis (and connect asyncronously)
        private static ISubscriber RedisSubscriber = RedisClient.GetSubscriber();
        private readonly Dictionary<string, RemoteIDEModel> RemoteModels = new Dictionary<string, RemoteIDEModel>();
        public IEnumerable<SourceControlledDocumentData> GetOpenFiles() => RemoteModels.Values.SelectMany(model => model.OpenFiles.Select(of => new SourceControlledDocumentData()
        { // TODO: Make this part of a base class
            Repository = of.RepoUrl,
            IdeUserIdentity = model.IDEUserIdentity,
            RelativePath = of.RelativePath,
            BeingEdited = of.BeingEdited,
            HasFocus = of == model.OpenFiles.OrderByDescending(oof => oof.LastActioned).FirstOrDefault()
        }));
        public RedisRemoteModelPersister()
        {
            RedisSubscriber.Subscribe(ModelPersisterChannel, OnRemoteModelReceived);
        }

        public event EventHandler RemoteModelReceived;

        private void OnRemoteModelReceived(RedisChannel channel, RedisValue value)
        {
            using (var ms = new MemoryStream(value))
            {
                var RemoteModel = ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(ms);
                RemoteModels[RemoteModel.Id] = RemoteModel;
            }

            RemoteModelReceived?.Invoke(this, EventArgs.Empty);
        }
        public void Dispose()
        {
            RedisClient?.Dispose();
        }
    }
}
