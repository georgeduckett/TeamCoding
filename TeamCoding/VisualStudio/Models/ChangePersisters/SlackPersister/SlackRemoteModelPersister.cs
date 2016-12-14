using SlackConnector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private void Slack_RemoteModelReceived(SlackMessage message)
        {
            try
            {
                var receivedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<BotMessage>(message.RawData);

                var receivedModel = TeamCodingPackage.Current.ObjectSlackMessageConverter.ToIdeModel(receivedMessage);

                receivedModel.IDEUserIdentity.ImageUrl = receivedModel.IDEUserIdentity.ImageUrl?.TrimStart('<')?.TrimEnd('>');
                if (receivedModel.IDEUserIdentity.DisplayName?.Contains("|") ?? false)
                {
                    receivedModel.IDEUserIdentity.DisplayName = receivedModel.IDEUserIdentity.DisplayName?.Substring(receivedModel.IDEUserIdentity.DisplayName.IndexOf('|') + 1).TrimEnd('>');
                }
                if (receivedModel.IDEUserIdentity.Id.Contains("|"))
                {
                    receivedModel.IDEUserIdentity.Id = receivedModel.IDEUserIdentity.Id.Substring(receivedModel.IDEUserIdentity.Id.IndexOf('|') + 1).TrimEnd('>');
                }

                foreach (var openFile in receivedModel.OpenFiles)
                {
                    openFile.RepoUrl = openFile.RepoUrl?.TrimStart('<')?.TrimEnd('>');
                }

                OnRemoteModelReceived(receivedModel);
            }
            catch(Exception ex)
            {
                TeamCodingPackage.Current.Logger.WriteError("Error parsing Slack Message", ex);
            }
        }
        

        public override void Dispose()
        {
            Task.WaitAll(SubscribeTask);
            base.Dispose();
        }
    }
}
