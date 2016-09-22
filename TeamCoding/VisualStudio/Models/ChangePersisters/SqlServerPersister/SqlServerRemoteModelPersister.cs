using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using TeamCoding.Documents;
using Dapper;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister
{
    public class SqlServerRemoteModelPersister : RemoteModelPersisterBase
    {
        private readonly SqlConnectionWrapper ConnectionWrapper;
        public SqlServerRemoteModelPersister(SqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
            ConnectionWrapper.DataChanged += ConnectionWrapper_DataChanged;
        }
        private void ConnectionWrapper_DataChanged(object sender, EventArgs e)
        { // TODO: Handle clearing connection string
            ClearRemoteModels();
            foreach(var queryData in ConnectionWrapper.GetData())
            {
                using (var ms = new MemoryStream(queryData.Model))
                {
                    OnRemoteModelReceived(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(ms));
                }
            }
        }
        public override void Dispose()
        {
            ConnectionWrapper.DataChanged -= ConnectionWrapper_DataChanged;
            base.Dispose();
        }
    }
}
