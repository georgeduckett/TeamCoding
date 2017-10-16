using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeamCoding.Extensions;

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
        private const string MergeQuery = @"MERGE [dbo].[TeamCodingSync] AS target
USING (VALUES (@Model, @LastUpdated))
    AS source (Model, LastUpdated)
    ON target.Id = @Id
WHEN MATCHED THEN
    UPDATE
    set Model = source.Model,
        LastUpdated = source.LastUpdated
WHEN NOT MATCHED THEN
    INSERT ( Id, model, LastUpdated)
    VALUES ( @Id,  @Model, @LastUpdated);";

        public event EventHandler DataChanged;
        private DateTime LastSqlWriteTime = DateTime.UtcNow;
        private Task RowHeartBeatTask;
        private CancellationTokenSource SqlHeartBeatCancelSource;
        private CancellationToken SqlHeartBeatCancelToken;
        private SqlWatcher TableWatcher;
        private SqlConnection GetConnection
        {
            get
            {
                try
                {
                    var con = new SqlConnection(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString);

                    con.Open();
                    return con;
                }
                catch(Exception ex)
                {
                    TeamCodingPackage.Current.Logger.WriteError(ex);
                    return null;
                }
            }
        }
        private bool ConnectionStringWorking(string connectionString)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static async Task<string> GetConnectionStringErrorTextAsync(string connectionString)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    var id = "TEST_" + DateTime.UtcNow.ToString();

                    try
                    {
                        if(con.Execute(MergeQuery, new { Id = id, Model = (byte[])null, LastUpdated = DateTime.UtcNow }) != 1)
                        {
                            return "Failed to create an entry in the table; execute command didn't add a single row";
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        return "Failed to create an entry in the table." + Environment.NewLine + Environment.NewLine + ex.ToString();
                    }

                    try
                    {
                        var result = con.Query<QueryData>("SELECT [Id], [Model], [LastUpdated] FROM[dbo].[TeamCodingSync] WHERE [Id] = @Id", new { Id = id }).ToArray();

                        if(result.Length == 0)
                        {
                            return "Failed to read the test row in the table; query returned no rows.";
                        }
                        else if(result.Length != 1)
                        {
                            return "Failed to read the test row in the table; query returned multiple rows.";
                        }
                        else if(result.Single().Id != id)
                        {
                            return "Failed to read the test row in the table; Read id did not match.";
                        }
                    }
                    catch(Exception ex)
                    {
                        return "Failed to read the test row in the table." + Environment.NewLine + Environment.NewLine + ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return "Could not connect using the connection string" + Environment.NewLine + Environment.NewLine + ex.ToString();
            }
            return null;
        }
        public SqlConnectionWrapper()
        {
            CreateRowWatcher();
            TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionStringChanged += Settings_SqlServerConnectionStringChanged;
        }
        private void CreateRowWatcher()
        {
            var sqlServerConnectionString = TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString;
            if (ConnectionStringWorking(sqlServerConnectionString))
            {
                TableWatcher = new SqlWatcher(sqlServerConnectionString, SelectCommand);
                TableWatcher.DataChanged += TableWatcher_DataChanged;
                TableWatcher.Start();

                StartHeartBeatTask();
            }
        }
        private void StartHeartBeatTask()
        {
            SqlHeartBeatCancelSource?.Cancel();
            RowHeartBeatTask?.Wait();

            SqlHeartBeatCancelSource = new CancellationTokenSource();
            SqlHeartBeatCancelToken = SqlHeartBeatCancelSource.Token;
            RowHeartBeatTask = new Task(() =>
            {
                while (!SqlHeartBeatCancelToken.IsCancellationRequested)
                {
                    if (string.IsNullOrEmpty(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString))
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        try
                        {
                            var UTCNow = DateTime.UtcNow;
                            var Difference = (UTCNow - LastSqlWriteTime).TotalSeconds;
                            if (Difference > 60)
                            { // If there have been no changes in the last minute, update the row again (prevent it being tidied up by others)
                                using (var connection = GetConnection)
                                {
                                    connection?.Execute("UPDATE [dbo].[TeamCodingSync] SET LastUpdated = @LastUpdated WHERE Id = @Id", new { Id = LocalIDEModel.Id.Value, LastUpdated = UTCNow });
                                }
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
                }
            }, TaskCreationOptions.LongRunning);

            RowHeartBeatTask.Start();
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
            using (var connection = GetConnection)
            { // TODO: Handle an invalid / bad connection
                if (connection == null)
                {
                    return Enumerable.Empty<QueryData>();
                }
                else
                {
                    try
                    {
                        TableWatcher.DataChanged -= TableWatcher_DataChanged;

                        // Delete rows older than 90 seconds to clean up orphaned rows (VS crashes etc)
                        connection?.ExecuteWithLogging("DELETE FROM [dbo].[TeamCodingSync] WHERE DATEDIFF(SECOND, [LastUpdated], GETUTCDATE()) > 90");

                        TableWatcher.DataChanged += TableWatcher_DataChanged;

                        // Get the data
                        return connection.Query<QueryData>(SelectCommand, new { Id = LocalIDEModel.Id.Value });
                    }
                    catch(Exception ex)
                    {
                        TeamCodingPackage.Current.Logger.WriteError("Unable to get data", ex);
                        return Enumerable.Empty<QueryData>();
                    }
                }
            }
        }
        public void UpdateModel(RemoteIDEModel remoteModel)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, remoteModel);
                LastSqlWriteTime = DateTime.UtcNow;
                
                using(var connection = GetConnection)
                {
                    connection?.ExecuteWithLogging(MergeQuery, new QueryData() { Id = remoteModel.Id, Model = ms.ToArray(), LastUpdated = LastSqlWriteTime });
                }
            }
        }
        public void Dispose()
        {
            SqlHeartBeatCancelSource?.Cancel();
            if (ConnectionStringWorking(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString))
            {
                SqlDependency.Stop(TeamCodingPackage.Current.Settings.SharedSettings.SqlServerConnectionString);
                using (var connection = GetConnection)
                {
                    connection?.ExecuteWithLogging("DELETE FROM [dbo].[TeamCodingSync] WHERE Id = @Id", new { Id = LocalIDEModel.Id.Value });
                    // Delete any old sqldependency endpoints to prevent memory leaks
                    connection?.ExecuteWithLogging(@"DECLARE @ConvHandle uniqueidentifier
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
                }
            }
            RowHeartBeatTask?.Wait();
            if (TableWatcher != null)
            {
                TableWatcher.DataChanged -= TableWatcher_DataChanged;
            }
            TableWatcher?.Stop();
        }
    }
}
