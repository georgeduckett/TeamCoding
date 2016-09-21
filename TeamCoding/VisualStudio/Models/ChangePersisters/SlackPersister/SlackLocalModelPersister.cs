using SlackConnector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;
using TeamCoding.VisualStudio.Models.ChangePersisters;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister
{
    public class SlackLocalModelPersister : LocalModelPersisterBase
    {
        public SlackLocalModelPersister(LocalIDEModel model)
            :base(model,
                  TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenProperty,
                  TeamCodingPackage.Current.Settings.SharedSettings.SlackChannelProperty)
        {

        }
        protected override void SendModel(RemoteIDEModel remoteModel)
        {
            TeamCodingPackage.Current.Slack.Publish(TeamCodingPackage.Current.ObjectSlackMessageConverter.ToBotMessage(remoteModel)).HandleException();
        }
    }
}
