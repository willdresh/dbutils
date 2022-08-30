// // DBLookup.cs
// // Will Dresh
// // w@dresh.app

using System;
using System.Data;

namespace DBInterface
{

    /// <summary>
    /// DB Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal class DBLookup: Lookup, IEquatable<DBLookup>
    {
        private const int Hashcode_XOR_Operand = 2;

        internal DBLookup(string key, System.Data.IDbConnection dbConnection)
            : base(key)
        { DBConnection = dbConnection; }

        internal IDbConnection DBConnection { get; }

        public static DBLookup Build(ILookup query, DBLookupManager mgr)
        {
            return new DBLookup(query.Key, mgr.connection);
        }

        /// <summary>
        /// Adds an equality check for the reference-equality of <see cref="IDBLookup.DBConnection"/>
        /// when comparing instances of <c>IDBLookup</c>.
        /// </summary>
        /// <remarks>Short-circuits and returns false when
        /// <c>base.Equals(other as Lookup)</c> returns false</remarks>
        public bool Equals(DBLookup other)
        {
            return base.Equals(other as Lookup) &&
                ReferenceEquals(this.DBConnection, other.DBConnection);
        }

        public override bool Equals(object obj)
        {
            if (obj is DBLookup)
                return Equals(obj as DBLookup);
            if (obj is Lookup)
                return base.Equals(obj as Lookup);
            return false;
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode() ^ Hashcode_XOR_Operand;
            if (DBConnection != null)
                result ^= DBConnection.GetHashCode();
            return result;
        }
    }

    internal class DBLookupResult: LookupResult, ILookupResult<DBLookup>
    {
        internal DBLookupResult(DBLookup query, object response)
            : base(query, response)
        { }

        public new DBLookup Query { get { return base.Query as DBLookup; } }

        public static ILookupResult Build(ILookup query, DBLookupManager manager, object response)
        {
            return new DBLookupResult(DBLookup.Build(query, manager), response);
        }
    }
}
