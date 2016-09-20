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
            IdeModel.ModelChanged += IdeModel_ModelChanged;
        }
        private void IdeModel_ModelChanged(object sender, EventArgs e)
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
