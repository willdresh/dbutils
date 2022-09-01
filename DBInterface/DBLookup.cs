// DBLookup.cs
// Will Dresh
// w@dresh.app

// Testing edit from Visual Studio! 20220901
// See if the git repo is configured properly...

using System;
using System.Data;

namespace DBInterface
{

    /// <summary>
    /// DB Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal partial class DBLookup: DBLookupBase
    {
        private System.Data.IDbConnection cnx;
        internal override IDbConnection DBConnection { get => cnx; set => cnx = value; }

        internal DBLookup(string key, System.Data.IDbConnection dbConnection)
            : base(key)
        {
            cnx = dbConnection;
        }

        internal DBLookup(MutableDBLookup other)
            : base(other.Key_Internal)
        {
            cnx = other.Unwrap_Immutable.DBConnection;
        }

        /// <summary>
        /// Build an immutable query for use with the supplied manager.
        /// </summary>
        /// <param name="mgr">Mgr.</param>
        /// <param name="query">Query.</param>
        /// <returns>
        /// A newly-constructed immutable instance of <see cref="DBLookup"/>
        /// which <c>mgr</c> can use to perform lookups
        /// </returns>
        /// <exception cref="ArgumentNullException"><c>mgr</c> is <c>null</c>.</exception>
        public static DBLookup Build(DBLookupManager mgr, ILookup query)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            // Try to avoid calling KeyCopy by using interal reference, if possible
            string resultKey = (query is Lookup int_query) ? 
                int_query.Key_Internal : query?.KeyCopy;

            return new DBLookup(resultKey, mgr.connection);
        }

        /// <summary>
        /// Construct an immutable copy of the supplied instance of <see cref="DBLookupBase"/>.
        /// </summary>
        /// <param name="other">(NOT NULL) An instance of <see cref="DBLookupBase"/> to copy</param>
        /// <returns>
        /// A newly-constructed instance of <see cref="DBLookup"/> representing an immutable copy of <c>other</c>.
        /// </returns>
        /// <exception cref="DBLookupBugDetectedException"><c>other</c> is <c>null</c>.</exception>
        internal static DBLookup Copy_Internal(DBLookupBase other)
        {
            if (other == null)
                throw new DBLookupBugDetectedException(nameof(other));

            return new DBLookup(other.Key_Internal, other.DBConnection);
        }
    }
}
