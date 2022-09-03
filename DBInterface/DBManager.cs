using System;
using System.Data;


namespace DBInterface
{
    public delegate IDbConnection DBConnectionProvider(DeltaDBConnectionArgs args);

    [Flags]
    public enum DBConnectionPolicy
    {
        AUTO_CONNECT = 1,
        AUTO_DISCONNECT_ALL = 2,
        AUTO_DISCONNECT_WHEN_AUTOCONNECTED = 4
    }

    /// <summary>
    /// DBManager. This class cannot be externally inherited, as it has
    /// private-only construction. Note that building a DBManager
    /// always has a chance to throw exceptions (see <see cref="DBManager.Build(DBConnectionProvider, DBConnectionPolicy)"/>).
    /// </summary>
    public class DBManager: ILookupProvider<ILookup>
    {
        #region Test members - remove for production

        // RFP!
        public delegate DeltaDBConnectionArgs DbConnectionUpdated_For_XUnit(DeltaDBConnectionArgs args);

        // RFP!
        public static IDbConnection GetNewCnx_For_XUnit(IDbConnection oldCnx, IDbConnection newCnx)
        {
            return newCnx;
        }

        // These public events must be removed for deployment; they are added for testing purposes only
        // CTRL+F for the comment "RFP!" (remove for production)
        public event DbConnectionUpdated_For_XUnit XUnit_BefDBCnxCha; // RFP!
        public event DbConnectionUpdated_For_XUnit XUnit_AftDBCnxCha; // RFP!

        #endregion

        #region Nested Types

        internal delegate void DbConnectionUpdated(DeltaDBConnectionArgs args);

        internal class InvalidDBConnectionProvider: ExternalIntegrationException
        {
            public InvalidDBConnectionProvider(string str, Exception ex)
                :base(str, ex)
            { }
        }

        #endregion

        #region Static member data (defaults)
        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;
        #endregion

        #region Private member data

        private DBConnectionPolicy policy;
        private DBConnectionProvider cnxProvider;
        private DBLookupManager lookupMgr;

        #endregion

        #region Internal events

        internal event DbConnectionUpdated BeforeDBConnectionChanges;
        internal event DbConnectionUpdated AfterDBConnectionChanged;

        #endregion;


        /// <summary>
        /// Private constructor for DBManager. <br />
        /// All others should use <c>public static DBManager Build(...)</c> to construct instances
        /// </summary>
        /// <param name="cnx_Provider"></param>
        /// <exception cref="NoNullAllowedException">The supplied cnx_Provider function returned null</exception>
        /// <exception cref="InvalidDBConnectionProvider">The supplied cnx_Provider function threw an exception</exception>
        private DBManager(DBConnectionProvider cnx_Provider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            cnxProvider = cnx_Provider;
            
            IDbConnection conn;
            try
            {
                conn = cnxProvider(new DeltaDBConnectionArgs());
                if (conn == null) throw new NoNullAllowedException("in DBManagerManager.cs:78 - cnxProvider function returned null");
            } catch (Exception ex) { throw new InvalidDBConnectionProvider("in DBManager.cs:79 DBManager::DBManager(...)", ex); }

            #region Test Members - remove for production

            XUnit_AftDBCnxCha += (a) => a; // Prevent exceptions from being thrown by events with 0 subscribers
            XUnit_BefDBCnxCha += (a) => a; // Prevent exceptions from being thrown by events with 0 subscribers

            #endregion

            // Preload events so that they won't throw exceptions
            AfterDBConnectionChanged += ((a) => {
                // Automatically invoke our test event whenever our normal event is invoked
                // For production, just delete the following line and have empty braces for the function body of the lambda
                XUnit_AftDBCnxCha.Invoke(a); // RFP!
            });
            BeforeDBConnectionChanges += ((a) =>
            {
                // Automatically invoke our test event whenever our normal event is invoked
                // For production, just delete the following line and have empty braces for the function body of the lambda
                XUnit_BefDBCnxCha.Invoke(a); // RFP!
            });

            lookupMgr = new DBLookupManager(conn);
        }

        /// <summary>
        /// Private constructor for DBManager. <br />
        /// All others should use <c>public static DBManager Build(...)</c> to construct instances
        /// </summary>
        /// <param name="cnx_Provider"></param>
        /// <exception cref="ArgumentNullException"><c>cnx_Provider</c> is <c>null</c>.</exception>
        /// <exception cref="NoNullAllowedException">The supplied cnx_Provider function returned null</exception>
        /// <exception cref="InvalidDBConnectionProvider">The supplied cnx_Provider function threw an exception</exception>
        public static DBManager Build(DBConnectionProvider cnx_Provider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            if (cnx_Provider == null)
                throw new ArgumentNullException();
            return new DBManager(cnx_Provider, policy);
        }

        // TODO - should this really be public? Unsure
        // set to public for testing purposes
        public void NextConnection()
        {
            IDbConnection oldCnx = lookupMgr.connection,
                newCnx = cnxProvider(new DeltaDBConnectionArgs());

            BeforeDBConnectionChanges.Invoke(oldCnx, newCnx);
            lookupMgr.connection = newCnx;
            AfterDBConnectionChanged.Invoke(oldCnx, newCnx);
        }

        public ILookupResult<ILookup> Lookup(ILookup query)
        {
            if (!lookupMgr.DatabaseConnected) // TODO Leftoff - this needs to be a better check; we want to check "If the connection has already been used and is therefore no longer good"
                NextConnection();               // alternatively, modify the behavior of DBManager so that this can be a testable precondition (might be better)

            // TODO
            throw new NotImplementedException();
        }
    }
}
