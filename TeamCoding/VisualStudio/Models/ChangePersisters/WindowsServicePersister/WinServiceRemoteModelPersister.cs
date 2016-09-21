using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.WindowsServicePersister
{
    public class WinServiceRemoteModelPersister : RemoteModelPersisterBase
    {
        private readonly WinServiceClient Client;
        public WinServiceRemoteModelPersister(WinServiceClient client)
        {
            Client = client;
            Client.MessageReceived += Client_MessageReceived;
        }
        private void Client_MessageReceived(object sender, WinServiceClient.MessageReceivedEventArgs e)
        {
            using (var ms = new MemoryStream(e.Message))
            {
                OnRemoteModelReceived(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(ms));
            }
        }
        public override void Dispose()
        {
            Client.MessageReceived -= Client_MessageReceived;
            base.Dispose();
        }
    }
}
