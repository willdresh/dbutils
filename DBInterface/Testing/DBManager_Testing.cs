using System;
using System.Data;

namespace DBInterface
{
    partial class DBManager
    {
        #region Test members - remove for production

        // RFP!
        public delegate void DbConnectionUpdated_For_XUnit(DeltaDBConnectionArgs args);

        // RFP!
        public IDbConnection GetCnx_For_XUnit()
        {
            return lookupMgr.connection;
        }

        // These public events must be removed for deployment; they are added for testing purposes only
        // CTRL+F for the comment "RFP!" (remove for production)
        public event DbConnectionUpdated_For_XUnit XUnit_BefDBCnxCha; // RFP!
        public event DbConnectionUpdated_For_XUnit XUnit_AftDBCnxCha; // RFP!

        #endregion

    }
}
