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
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanging += SharedSettings_SlackServerChanging;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanged += SharedSettings_SlackServerChanged;
        }

        private void SharedSettings_SlackServerChanged(object sender, EventArgs e)
        {
            SendChanges();
        }

        private void SharedSettings_SlackServerChanging(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }

        private void IdeModel_TextDocumentSaved(object sender, Microsoft.VisualStudio.Text.TextDocumentFileActionEventArgs e)
        {
            SendChanges();
        }
        private void IdeModel_TextContentChanged(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            // SendChanges();
        }
        private void IdeModel_OpenViewsChanged(object sender, EventArgs e)
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
