using System;
using System.Data;


namespace DBInterface
{
    public delegate IDbConnection DBConnectionProvider(ObtainDbConnectionEventArgs args);

    [Flags]
    public enum DBConnectionPolicy
    {
        AUTO_CONNECT = 1,
        AUTO_DISCONNECT_ALL = 2,
        AUTO_DISCONNECT_WHEN_AUTOCONNECTED = 4
    }

    /// <summary>
    /// <c>public struct ObtainDbConnectionEventArgs</c>
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public struct ObtainDbConnectionEventArgs
    { }

    /// <summary>
    /// DBManager. This class cannot be externally inherited, as it has
    /// private-only construction. Note that building a DBManager
    /// always has a chance to throw exceptions (see <see cref="DBManager.Build(DBConnectionProvider, DBConnectionPolicy)"/>).
    /// </summary>
    public class DBManager: ILookupProvider<ILookup>
    {
        internal delegate void DbConnectionUpdated(IDbConnection oldCnx, IDbConnection newCnx);


        #region Test members - remove for production

        // RFP!
        public delegate IDbConnection DbConnectionUpdated_For_XUnit(IDbConnection oldCnx, IDbConnection newCnx);

        // RFP!
        public static IDbConnection GetNewCnx_For_XUnit(IDbConnection oldCnx, IDbConnection newCnx)
        {
            return newCnx;
        }
        #endregion

        #region Nested Types

        internal class InvalidDBConnectionProvider: ExternalIntegrationException
        {
            public InvalidDBConnectionProvider(string str, Exception ex)
                :base(str, ex)
            { }
        }

        #endregion

        // invariant_args will need to be removed if ObtainDbConnectionEventArgs is
        // ever changed to do something instance-specific
        private static readonly ObtainDbConnectionEventArgs invariant_args = new ObtainDbConnectionEventArgs();
        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        private DBConnectionPolicy policy;
        private DBConnectionProvider cnxProvider;
        private DBLookupManager lookupMgr;


        internal event DbConnectionUpdated BeforeDBConnectionChanges;
        internal event DbConnectionUpdated AfterDBConnectionChanged;

        // These public events must be removed for deployment; they are added for testing purposes only
        // CTRL+F for the comment "RFP!" (remove for production)
        public event DbConnectionUpdated_For_XUnit XUnit_BefDBCnxCha; // RFP!
        public event DbConnectionUpdated_For_XUnit XUnit_AftDBCnxCha; // RFP!

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
            AfterDBConnectionChanged += ((a, b) => { }); // Guarantee we never have 0 subscribers, to avoid unwanted exceptions when invoked
            BeforeDBConnectionChanges += ((a, b) => { }); // Guarantee we never have 0 subscribers, to avoid unwanted exceptions when invoked

            IDbConnection conn;
            try
            {
                conn = cnxProvider(invariant_args);
                if (conn == null) throw new NoNullAllowedException("in DBManagerManager.cs:78 - cnxProvider function returned null");
            } catch (Exception ex) { throw new InvalidDBConnectionProvider("in DBManager.cs:79 DBManager::DBManager(...)", ex); }

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

        public static DBManager Build_For_TestDummy(TestDummy dummy, DBConnectionProvider cnx_Provider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {


            return Build(cnx_Provider, policy);
        }

        // TODO - should this really be public? Unsure
        // set to public for testing purposes
        public void NextConnection()
        {
            IDbConnection oldCnx = lookupMgr.connection,
                newCnx = cnxProvider(new ObtainDbConnectionEventArgs());

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
