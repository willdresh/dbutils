// // DBLookupManager.cs
// // Will Dresh
// // w@dresh.app
using System;
using System.Data;

namespace DBInterface
{
    public partial class DBLookupManager: LookupManager
    {
        public delegate IDbConnection DBConnectionProvider();

        [Flags]
        public enum DBConnectionPolicy
        {
            AUTO_CONNECT = 1,
            AUTO_DISCONNECT_ALL = 2,
            AUTO_DISCONNECT_WHEN_AUTOCONNECTED = 4,

            /// <summary>
            /// Implementation in-progress
            /// </summary>
            AUTO_REFRESH = 8
        }

        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        internal IDbConnection connection;
        private ILookupProvider<DBLookupBase> dblp;
        protected DBConnectionProvider connectionProvider;

        public DBConnectionPolicy ConnectionPolicy { get; }

        /// <summary>
        /// Get the state of the connection managed by this instance. May throw exception
        /// if this instance currently does not own any connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance does not currently manage a <c>System.Data.IDbConnection</c>.
        /// </exception>
        /// <seealso cref="IsManagingConnection"/>
        public ConnectionState ConnectionState { get {
                if (connection == null)
                    throw new InvalidOperationException();
                return connection.State;
        } }

        /// <summary>
        /// Get a value indicating whether or not this instance currently manages a
        /// <c>System.Data.IDbConnection</c>.
        /// </summary>
        public bool IsManagingConnection { get
            {
                return connection != null;
            } }

        /// <summary>
        /// Get a value indicating whether or not the <c>System.Data.IDbConnection</c> managed
        /// by this instance is currently open (connected).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance does not currently manage a <c>System.Data.IDbConnection</c>.
        /// </exception>
        /// <seealso cref="IsManagingConnection"/>
        public bool DatabaseConnected { get {
                if (connection == null)
                    throw new InvalidOperationException();

                return ConnectionState.HasFlag(ConnectionState.Open);
        } }

        public DBLookupManager(DBConnectionProvider connectionProvider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            connection = null;
        }

        public DBLookupManager(IDbConnection _connection, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            connection = _connection;
            dblp = new DBLookupProvider();
            ConnectionPolicy = policy;
        }

        internal DBLookupManager(IDbConnection _connection, ILookupProvider<DBLookupBase> databaseLookupProvider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
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
            return new DBLookup(query.KeyCopy, connection);
        }

        public DBLookupBase BuildLookupBase(ILookup query)
        {
            return BuildLookup(query);
        }

        internal DBLookupResult BuildFailureInstance_Internal(DBLookupBase query)
        {
            if (query == null)
                throw new DBLookupManagerBugDetectedException(nameof(BuildFailureInstance_Internal), nameof(query));

            return DBLookupResult.Build_Internal(query, null);
        }

        /// <summary>
        /// Performs a lookup using the internally-wrapped database connection.<br />
        /// If the connection state (<see cref="DBLookupBase.DBConnectionState"/>) is not already open,
        ///  AND policy allows, this may trigger an auto-connect.<br />
        /// It can also trigger auto-disconnects according to policy.
        /// </summary>
        /// <returns>The lookup.</returns>
        /// <param name="query">Query.</param>
        /// <exception cref="InternalInstanceExpectedException"></exception>
        internal DBLookupResult DatabaseLookup(DBLookup query)
        {
            DBLookupResult result;
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
                else result = BuildFailureInstance_Internal(query);
                //else throw new PolicyProhibitsAutoConnectException(); //Deprecated
            }

            try
            {
                //TODO
                // this could use improvement

                if (dblp is DBLookupProvider provider)
                    result = provider.Lookup_Internal(query);
                else
                {
                    ILookupResult<DBLookupBase> lookupResult = dblp.Lookup(query);
                    if (lookupResult is DBLookupResult success)
                        result = success;
                    else throw new InternalInstanceExpectedException("Runtime type returned by DBLookupManager.dblp.Lookup(DBLookup) was not an instance of DBLookupResult. This could be because this instance's dblp has an externally-defined type.");
                }
            }
            finally { if (finallyCloseConnection) ExecuteAutoDisconnect(); }

            return result;
        }

        internal DBLookupResult Lookup_Internal(DBLookup query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return DatabaseLookup(query);
        }

        protected override ILookupResult<ILookup> DoLookup(ILookup query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return Lookup_Internal(BuildLookup(query));
        }
    }
}
