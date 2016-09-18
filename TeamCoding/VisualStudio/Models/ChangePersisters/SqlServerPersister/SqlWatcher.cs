using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SqlServerPersister
{
    public class SqlWatcher
    { // http://stackoverflow.com/questions/20503286/using-sqldependency-with-named-queues
        private readonly string ConnectionString;
        private readonly string ListenerQuery;
        private SqlDependency Dependency;
        public event EventHandler DataChanged;
        public SqlWatcher(string connectionString, string listenerQuery)
        {
            ConnectionString = connectionString;
            ListenerQuery = listenerQuery;
        }
        public void Start()
        {
            SqlDependency.Start(ConnectionString);
            ListenForChanges();
        }
        public void Stop()
        {
            SqlDependency.Stop(ConnectionString);
        }
        private void ListenForChanges()
        {
            //Remove existing dependency, if necessary
            if (Dependency != null)
            {
                Dependency.OnChange -= Dependency_OnChange;
                Dependency = null;
            }

            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            SqlCommand command = new SqlCommand(ListenerQuery, connection);

            Dependency = new SqlDependency(command);

            // Subscribe to the SqlDependency event.
            Dependency.OnChange += Dependency_OnChange;

            SqlDependency.Start(ConnectionString);

            command.ExecuteReader();

            //Perform this action when SQL notifies of a change
            DataChanged?.Invoke(this, EventArgs.Empty);

            connection.Close();
        }

        private void Dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Source == SqlNotificationSource.Data || e.Source == SqlNotificationSource.Timeout)
            {
                ListenForChanges();
            }
            else
            {
                TeamCodingPackage.Current.Logger.WriteError($"Data not refreshed due to unexpected SqlNotificationEventArgs: Source={e.Source}, Info={e.Info}, Type={e.Type}");
            }
        }
    }
}
