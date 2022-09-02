using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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

    public class DBManager
    {
        private DbConnectionProvider cnxProvider;
        private DBLookupManager lookupMgr;
    }
}
