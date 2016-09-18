using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister
{
    public class SqlConnectionWrapper : IDisposable
    {
        public class QueryData
        {
            public string Id { get; set; }
            public byte[] Model { get; set; }
            private DateTime _LastUpdated;
            public DateTime LastUpdated
            {
                get { return _LastUpdated; }
                set { _LastUpdated = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
            }
        }

        private const string SelectCommand = "SELECT [Id], [Model], [LastUpdated] FROM [dbo].[TeamCodingSync]";
        private SqlConnection TableWatcherConnection;
        public event EventHandler DataChanged;
        private DateTime LastSqlWriteTime = DateTime.UtcNow;
        private readonly Thread RowHeartBeatThread;
        private CancellationTokenSource SqlHeartBeatCancelSource;
        private CancellationToken SqlHeartBeatCancelToken;
        private SqlWatcher TableWatcher;
        public SqlConnectionWrapper()
        {
            CreateRowWatcher();
            TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionStringChanged += Settings_SqlServerConnectionStringChanged;

            SqlHeartBeatCancelSource = new CancellationTokenSource();
            SqlHeartBeatCancelToken = SqlHeartBeatCancelSource.Token;
            RowHeartBeatThread = new Thread(() =>
            {
                while (!SqlHeartBeatCancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var UTCNow = DateTime.UtcNow;
                        var Difference = (UTCNow - LastSqlWriteTime).TotalSeconds;
                        if (Difference > 60)
                        { // If there have been no changes in the last minute, update the row again (prevent it being tidied up by others)
                            TableWatcherConnection?.Execute("UPDATE [dbo].[TeamCodingSync] SET LastUpdated = @LastUpdated WHERE Id = @Id", new { Id = LocalIDEModel.Id.Value, LastUpdated = UTCNow });
                            LastSqlWriteTime = UTCNow;
                            SqlHeartBeatCancelToken.WaitHandle.WaitOne(1000 * 60);
                        }
                        else
                        {
                            SqlHeartBeatCancelToken.WaitHandle.WaitOne(1000 * (60 - (int)Difference + 1));
                        }
                    }
                    catch (SqlException)
                    {
                        SqlHeartBeatCancelToken.WaitHandle.WaitOne(10000);
                    }
                }
            });

            RowHeartBeatThread.Start();
        }
        private void CreateRowWatcher()
        {
            if (!string.IsNullOrEmpty(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString))
            {
                TableWatcherConnection = new SqlConnection(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString);
                TableWatcherConnection.Open();
                SqlDependency.Start(TableWatcherConnection.ConnectionString);
                TableWatcher = new SqlWatcher(TableWatcherConnection.ConnectionString, SelectCommand);
                TableWatcher.DataChanged += TableWatcher_DataChanged;
                TableWatcher.Start();
            }
        }

        private void TableWatcher_DataChanged(object sender, EventArgs e)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        private void Settings_SqlServerConnectionStringChanged(object sender, EventArgs e)
        {
            Dispose();
            CreateRowWatcher();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        public IEnumerable<QueryData> GetData()
        {
            TableWatcher.DataChanged -= TableWatcher_DataChanged;
            // Delete rows older than 90 seconds to clean up orphaned rows (VS crashes etc)
            TableWatcherConnection.Execute("DELETE FROM [dbo].[TeamCodingSync] WHERE DATEDIFF(SECOND, [LastUpdated], GETUTCDATE()) > 90");
            TableWatcher.DataChanged += TableWatcher_DataChanged;
            // Get the data
            return TableWatcherConnection.Query<QueryData>(SelectCommand, new { Id = LocalIDEModel.Id.Value });
        }
        public void UpdateModel(RemoteIDEModel remoteModel)
        {
            if (TableWatcherConnection == null) return;

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, remoteModel);
                LastSqlWriteTime = DateTime.UtcNow;
                try
                {
                    TableWatcherConnection.Execute(@"MERGE [dbo].[TeamCodingSync] AS target
USING (VALUES (@Model, @LastUpdated))
    AS source (Model, LastUpdated)
    ON target.Id = @Id
WHEN MATCHED THEN
    UPDATE
    set Model = source.Model,
        LastUpdated = source.LastUpdated
WHEN NOT MATCHED THEN
    INSERT ( Id, model, LastUpdated)
    VALUES ( @Id,  @Model, @LastUpdated);", new QueryData() { Id = remoteModel.Id, Model = ms.ToArray(), LastUpdated = LastSqlWriteTime });
                }
                catch (SqlException ex)
                {
                    TeamCodingPackage.Current.Logger.WriteError("Failed to create sql row.", ex);
                }
            }
        }
        public void Dispose()
        {
            SqlDependency.Stop(TableWatcherConnection.ConnectionString);
            SqlHeartBeatCancelSource.Cancel();
            RowHeartBeatThread.Join();
            if (TableWatcher != null)
            {
                TableWatcher.DataChanged -= TableWatcher_DataChanged;
            }
            TableWatcherConnection?.Execute("DELETE FROM [dbo].[TeamCodingSync] WHERE Id = @Id", new { Id = LocalIDEModel.Id.Value });
            // Delete any old sqldependency endpoints to prevent memory leaks
            TableWatcherConnection?.Execute(@"DECLARE @ConvHandle uniqueidentifier
DECLARE Conv CURSOR FOR
SELECT CEP.conversation_handle FROM sys.conversation_endpoints CEP
WHERE CEP.state = 'DI' or CEP.state = 'CD'
OPEN Conv;
FETCH NEXT FROM Conv INTO @ConvHandle;
WHILE (@@FETCH_STATUS = 0) BEGIN
    END CONVERSATION @ConvHandle WITH CLEANUP;
    FETCH NEXT FROM Conv INTO @ConvHandle;
END
CLOSE Conv;
DEALLOCATE Conv;");
            TableWatcher.Stop();
            TableWatcherConnection.Close();
            TableWatcherConnection?.Dispose();
        }
    }
}
