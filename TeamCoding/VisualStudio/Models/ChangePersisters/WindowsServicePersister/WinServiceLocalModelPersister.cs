using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Options;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.WindowsServicePersister
{
    public class WinServiceLocalModelPersister : LocalModelPersisterBase
    {
        private readonly WinServiceClient Client;
        public WinServiceLocalModelPersister(WinServiceClient client, LocalIDEModel model) : base(model, TeamCodingPackage.Current.Settings.SharedSettings.WinServiceIPAddressProperty)
        {
            Client = client;
        }
        protected override void SendModel(RemoteIDEModel remoteModel)
        {
            Client.SendModel(remoteModel);
        }
    }
}
