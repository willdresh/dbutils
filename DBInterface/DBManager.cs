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
    /// no public/protected constructors.
    /// </summary>
    public class DBManager: ILookupProvider<ILookup>
    {
        internal delegate void DbConnectionUpdated(IDbConnection oldCnx, IDbConnection newCnx);

        internal class InvalidDBConnectionProvider: ExternalIntegrationException
        {
            public InvalidDBConnectionProvider(string str, Exception ex)
                :base(str, ex)
            { }
        }

        // invariant_args will need to be removed if ObtainDbConnectionEventArgs is
        // ever changed to do something instance-specific
        private static readonly ObtainDbConnectionEventArgs invariant_args = new ObtainDbConnectionEventArgs();
        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        private DBConnectionPolicy policy;
        private DBConnectionProvider cnxProvider;
        private DBLookupManager lookupMgr;
        private bool cnxUsed;
;

        internal event DbConnectionUpdated BeforeDBConnectionChanges;
        internal event DbConnectionUpdated AfterDBConnectionChanged;

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
                if (conn == null) throw new NoNullAllowedException();
            } catch (Exception ex) { throw new InvalidDBConnectionProvider("in DBManager.cs: DBManager::DBManager(...)", ex); }

            lookupMgr = new DBLookupManager(conn);
        }

        /// <summary>
        /// Private constructor for DBManager. <br />
        /// All others should use <c>public static DBManager Build(...)</c> to construct instances
        /// </summary>
        /// <param name="cnx_Provider"></param>
        /// <exception cref="ArgumentNullException"><c>cnx_Provider</c> is <c>null</c>.</exception>
        /// <exception cref="NoNullAllowedException">The supplied cnx_Provider function returned null</exception>
        /// <exception cref="InvalidDbConnectionProvider">The supplied cnx_Provider function threw an exception</exception>
        public static DBManager Build(DBConnectionProvider cnx_Provider, DBConnectionPolicy policy = DEFAULT_Connection_Policy)
        {
            if (cnx_Provider == null)
                throw new ArgumentNullException();
            return new DBManager(cnx_Provider, policy);
        }

        private void NextConnection()
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
