using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamCoding.Documents;
using Dapper;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister
{
    public class SqlServerLocalModelPersister : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        private readonly SqlConnectionWrapper ConnectionWrapper;
        public SqlServerLocalModelPersister(SqlConnectionWrapper connectionWrapper, LocalIDEModel model)
        {
            ConnectionWrapper = connectionWrapper;
            IdeModel = model;
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;
        }
        private void SharedSettings_FileBasedPersisterPathChanging(object sender, EventArgs e)
        {
            SendEmpty();
        }
        private void Settings_FileBasedPersisterPathChanged(object sender, EventArgs e)
        {
            SendChanges();
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
        protected virtual void SendEmpty()
        {
            SendIdeModel(new RemoteIDEModel(new LocalIDEModel()));
        }
        protected virtual void SendChanges()
        {
            SendIdeModel(new RemoteIDEModel(IdeModel));
        }
        private void SendIdeModel(RemoteIDEModel remoteModel)
        {
            ConnectionWrapper.UpdateModel(remoteModel);
        }
        public void Dispose() { }
    }
}
