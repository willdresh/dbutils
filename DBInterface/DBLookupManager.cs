// // DBLookupManager.cs
// // Will Dresh
// // w@dresh.app
using System;
using System.Data;

namespace DBInterface
{
    public class DBLookupManager: LookupManager, ILookupProvider
    {
        [Flags]
        public enum DBConnectionPolicy
        {
            AUTO_CONNECT = 1,
            AUTO_DISCONNECT_ALL = 2,
            AUTO_DISCONNECT_WHEN_AUTOCONNECTED = 4,

            /// <summary>
            /// Functionality not yet implemented
            /// </summary>
            AUTO_REFRESH = 8
        }

        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        internal IDbConnection connection;
        private ILookupProvider<DBLookup> dblp;

        public DBConnectionPolicy ConnectionPolicy { get; }
        public ConnectionState ConnectionState { get { return connection.State; } }
        public bool DatabaseConnected { get { return ConnectionState.HasFlag(ConnectionState.Open); } }

        public DBLookupManager(IDbConnection _connection, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            connection = _connection;
            dblp = new DBLookupProvider();
            ConnectionPolicy = policy;
        }

        internal DBLookupManager(IDbConnection _connection, ILookupProvider<DBLookup> databaseLookupProvider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            connection = _connection;
            dblp = databaseLookupProvider;
            ConnectionPolicy = policy;
        }

        private void ExecuteAutoDisconnect()
        {
            connection.Close();
        }

        private void ExecuteAutoConnect()
        {
            connection.Open();
        }

        private DBLookup BuildLookup(ILookup query)
        {
            return new DBLookup(query.Key, connection);
        }

        internal DBLookupResult DatabaseLookup(DBLookup query)
        {
            // FIRST: ready our database connection
            bool finallyCloseConnection = ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_DISCONNECT_ALL);
            if (!DatabaseConnected)
            {
                if (ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_CONNECT))
                {
                    ExecuteAutoConnect();
                    finallyCloseConnection = finallyCloseConnection
                        || ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED);
                }
                else throw new PolicyProhibitsAutoConnectException();
            }

            DBLookupResult result;
            try
            {
                //TODO
                // this could use improvement

                if (dblp is DBLookupProvider provider)
                    result = provider.Lookup_Internal(query);
                else
                    result = new DBLookupResult(query, dblp.Lookup(query).Response);
            }
            finally { if (finallyCloseConnection) ExecuteAutoDisconnect(); }

            return result;
        }

        internal DBLookupResult Lookup_Internal(DBLookup query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return DatabaseLookup(query);
        }

        protected override ILookupResult DoLookup(ILookup query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return Lookup_Internal(BuildLookup(query));
        }
    }
}
