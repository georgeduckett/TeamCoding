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
            // TODO: Parse the message text and call OnRemoteModelReceived
        }
        public override void Dispose()
        {
            Task.WaitAll(SubscribeTask);
            base.Dispose();
        }
    }
}
