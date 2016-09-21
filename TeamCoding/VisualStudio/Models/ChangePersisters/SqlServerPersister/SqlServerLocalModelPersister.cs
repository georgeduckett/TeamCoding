using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamCoding.Documents;
using Dapper;
using TeamCoding.VisualStudio.Models.ChangePersisters;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister
{
    public class SqlServerLocalModelPersister : LocalModelPersisterBase
    {
        private readonly SqlConnectionWrapper ConnectionWrapper;
        public SqlServerLocalModelPersister(SqlConnectionWrapper connectionWrapper, LocalIDEModel model)
            :base(model, TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionStringProperty)
        {
            ConnectionWrapper = connectionWrapper;
        }
        protected override void SendModel(RemoteIDEModel remoteModel) => ConnectionWrapper.UpdateModel(remoteModel);
    }
}
