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
    internal partial class DBLookup: DBLookupBase
    {
        internal DBLookup(string key, System.Data.IDbConnection dbConnection)
            : base(key, dbConnection)
        { }

        internal DBLookup(MutableDBLookup other)
            : base(other.Key_Internal, other.Unwrap_Immutable.DBConnection)
        { }

        /// <summary>
        /// Try to avoid using this constructor as it needs
        /// two calls to ImmutableCopy(). Other constructors for this class
        /// do not need to create any copies.
        /// </summary>
     //   internal DBLookup(IMutableLookup<DBLookupBase> other)
    	//	:base(other.ImmutableCopy().Key_Internal, other.ImmutableCopy().DBConnection)
    	//{ }

        /// <summary>
        /// Build an immutable query for use with the supplied manager.
        /// </summary>
        /// <param name="mgr">Mgr.</param>
        /// <param name="query">Query.</param>
        /// <returns>
        /// A newly-constructed immutable instance of <see cref="DBLookup"/>
        /// which <c>mgr</c> can use to perform lookups
        /// </returns>
        internal static DBLookup Build(DBLookupManager mgr, ILookup query)
        {
            if (mgr == null)
                throw new DBLookupBugDetectedException(nameof(mgr));
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            string resultKey = (query is Lookup int_query) ? // Try to avoid calling KeyCopy by using interal reference, if possible
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
