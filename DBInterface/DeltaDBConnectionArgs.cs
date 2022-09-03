using System;
using System.Data;
using System.Runtime.InteropServices;

namespace DBInterface
{
    [Serializable]
    [ComVisible(true)]
    public struct DeltaDBConnectionArgs
    {
        public DeltaDBConnectionArgs(IDbConnection _oldCnx, IDbConnection _newCnx)
        {
            OldCnx = _oldCnx;
            NewCnx = _newCnx;
        }

        public IDbConnection OldCnx { get; set; }
        public IDbConnection NewCnx { get; set; }
    }
}
