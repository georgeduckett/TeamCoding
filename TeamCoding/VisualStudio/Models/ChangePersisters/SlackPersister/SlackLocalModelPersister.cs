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
    public class SlackLocalModelPersister : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        public SlackLocalModelPersister(LocalIDEModel model)
        {
            IdeModel = model;
            IdeModel.ModelChanged += IdeModel_ModelChanged;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanged += IdeModel_ModelChanged;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanging += SharedSettings_SlackServerChanging;
        }
        private void SharedSettings_SlackServerChanging(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }
        private void IdeModel_ModelChanged(object sender, EventArgs e)
        {
            SendChanges();
        }
        protected virtual void SendChanges()
        {
            SendModel(new RemoteIDEModel(IdeModel));
        }
        private void SendModel(RemoteIDEModel remoteModel)
        {
            TeamCodingPackage.Current.Logger.WriteInformation("Publishing Model");
            TeamCodingPackage.Current.Slack.Publish(TeamCodingPackage.Current.ObjectSlackMessageConverter.ToBotMessage(remoteModel)).HandleException();
        }
        public void Dispose()
        {

        }
    }
}
