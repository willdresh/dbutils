using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace DBInterface
{
    public delegate IDbConnection DbConnectionProvider(ObtainDbConnectionEventArgs args);

    [Flags]
    public enum DBConnectionPolicy
    {
        AUTO_CONNECT = 1,
        AUTO_DISCONNECT_ALL = 2,
        AUTO_DISCONNECT_WHEN_AUTOCONNECTED = 4
    }

    [Serializable]
    [ComVisible(true)]
    public struct ObtainDbConnectionEventArgs
    { }

    public class DBManager: ILookupProvider<ILookup>
    {
        internal delegate void DbConnectionUpdated(IDbConnection oldCnx, IDbConnection newCnx);

        public const DBConnectionPolicy DEFAULT_Connection_Policy =
            DBConnectionPolicy.AUTO_CONNECT
            | DBConnectionPolicy.AUTO_DISCONNECT_WHEN_AUTOCONNECTED;

        private DBConnectionPolicy policy;
        private DbConnectionProvider cnxProvider;
        private DBLookupManager lookupMgr;

        internal event DbConnectionUpdated AfterDBConnectionChanged;

        public DBManager(DbConnectionProvider cnx_Provider)
        {
            cnxProvider = cnx_Provider;
            lookupMgr = new DBLookupManager(null);
            AfterDBConnectionChanged += ((a, b) => { }); // Don't throw exceptions when event has 0 subscribers
        }

        private void NextConnection()
        {
            IDbConnection oldCnx = lookupMgr.connection, newCnx;
            lookupMgr.connection = newCnx = cnxProvider(new ObtainDbConnectionEventArgs());
            AfterDBConnectionChanged.Invoke(oldCnx, newCnx);
        }

        public ILookupResult<ILookup> Lookup(ILookup query)
        {
            if (!lookupMgr.DatabaseConnected) // TODO Leftoff - this needs to be a better check; we want to check "If the connection has already been used and is therefore no longer good"
                NextConnection();               // alternatively, modify the behavior of DBManager so that this can be a precondition (might be easier)

            // TODO
            throw new NotImplementedException();
        }
    }
}
