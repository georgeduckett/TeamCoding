using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister
{
    public class SlackRemoteModelPersister : RemoteModelPersisterBase
    {
        private readonly Task SubscribeTask;
        public SlackRemoteModelPersister()
        {
            SubscribeTask = TeamCodingPackage.Current.Slack.Subscribe(Slack_RemoteModelReceived).HandleException();
        }
        private void Slack_RemoteModelReceived(string messageText)
        {
            // TODO: Handle an invalid message being received
            var receivedModel = Newtonsoft.Json.JsonConvert.DeserializeObject<RemoteIDEModel>(messageText);

            receivedModel.IDEUserIdentity.ImageUrl = receivedModel.IDEUserIdentity.ImageUrl.TrimStart('<').TrimEnd('>');
            if (receivedModel.IDEUserIdentity.DisplayName.Contains("|"))
            {
                receivedModel.IDEUserIdentity.DisplayName = receivedModel.IDEUserIdentity.DisplayName.Substring(receivedModel.IDEUserIdentity.DisplayName.IndexOf('|') + 1).TrimEnd('>');
            }
            if (receivedModel.IDEUserIdentity.Id.Contains("|"))
            {
                receivedModel.IDEUserIdentity.Id = receivedModel.IDEUserIdentity.Id.Substring(receivedModel.IDEUserIdentity.Id.IndexOf('|') + 1).TrimEnd('>');
            }

            foreach (var openFile in receivedModel.OpenFiles)
            {
                openFile.RepoUrl = openFile.RepoUrl.TrimStart('<').TrimEnd('>');
            }

            OnRemoteModelReceived(receivedModel);
        }
        public override void Dispose()
        {
            Task.WaitAll(SubscribeTask);
            base.Dispose();
        }
    }
}
