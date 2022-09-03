// // DBLookupManager.cs
// // Will Dresh
// // w@dresh.app
using System;
using System.Data;

namespace DBInterface
{
    public partial class DBLookupManager: LookupManager
    {
        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        internal IDbConnection connection;
        private ILookupProvider<DBLookupBase> dblp;

        public DBConnectionPolicy ConnectionPolicy { get; }
        public ConnectionState ConnectionState { get { return connection.State; } }
        public bool DatabaseConnected { get { return ConnectionState.HasFlag(ConnectionState.Open); } }

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

        /// <exception cref="DBInterfaceApplicationException" />
        private void ExecuteAutoDisconnect()
        {
            try {
                connection.Close();
            }
            catch (Exception ex)
            { throw new DBInterfaceApplicationException("An error occurred while trying to auto-disconnect", ex); }
        }

        /// <exception cref="DBInterfaceApplicationException" />
        private void ExecuteAutoConnect()
        {
            try {
                connection.Open();
            } catch (Exception ex)
            { throw new DBInterfaceApplicationException("An error occurred while trying to auto-connect", ex); }
        }


        /// <exception cref="ArgumentNullException" />
        private DBLookup BuildLookup(ILookup query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return new DBLookup(query.KeyCopy, connection);
        }


        /// <exception cref="ArgumentNullException" />
        public DBLookupBase BuildLookupBase(ILookup query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return BuildLookup(query);
        }


        /// <exception cref="ArgumentNullException" />
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
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InternalInstanceExpectedException">
        /// Tried to use a custom externally-defined provider of <see cref="ILookupProvider{DBLookupBase}"/>
        /// (this behavior is not yet supported, but may be in future versions)
        /// </exception>
        /// <exception cref="DBInterfaceApplicationException">
        /// The <c>System.Data.IDbConnection</c> threw an exception while
        /// trying to auto-disconnect
        /// </exception>
        /// <exception cref="DataUnreachableException">
        /// The <c>System.Data.IDbConnection</c> threw an exception while
        /// trying to auto-connect
        /// </exception>
        /// <exception cref="UnexpectedBehaviorException">
        /// The <c>System.Data.IDbConnection</c> was successfully connected,
        /// but had improper/unexpected <c>ConnectionState</c> flags.
        /// </exception>
        public DBLookupResult DatabaseLookup(DBLookup query)
            //TODO - change to internal or MAYBE protected internal.
            //was changed to public for unit testing
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            DBLookupResult result;
            // FIRST: ready our database connection
            bool finallyCloseConnection = ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_DISCONNECT_ALL);
            if (!DatabaseConnected)
            {
                if (ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_CONNECT))
                {
                    try { ExecuteAutoConnect(); }
                    catch (DBInterfaceApplicationException ex) { throw new DataUnreachableException("Unable to obtain connection for lookup", ex); }
                    finallyCloseConnection = finallyCloseConnection
                        || ConnectionPolicy.HasFlag(DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED);
                }

                // If policy prohibits auto-connect, and we are not already connected,
                // then return a failure result
                else result = BuildFailureInstance_Internal(query);
            }


            // Our connection is now open

            if (ConnectionState.HasFlag(ConnectionState.Broken)
                || ConnectionState.HasFlag(ConnectionState.Closed)
                || !ConnectionState.HasFlag(ConnectionState.Open))
                throw new UnexpectedBehaviorException("Invalid connection state (this is most likely caused by an external provider of System.Data.IDbConnection misbehaving)");

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
            finally {
                if (finallyCloseConnection)
                    try { ExecuteAutoDisconnect(); }
                    catch (DBInterfaceApplicationException ex) { throw new DBInterfaceApplicationException("A problem occurred while trying to close the connection", ex); }
            }

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
