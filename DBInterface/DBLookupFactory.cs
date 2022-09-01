// // DBLookupFactory.cs
// // Will Dresh
// // w@dresh.app
using System;
namespace DBInterface
{

    /// <summary>
    /// Simple factory class to provide <see cref="T:DBLookupBase"/> instances for
    /// use with <see cref="T:DBLookupManager"/>
    /// </summary>
    public static class DBLookupFactory
    {
        /// <summary>
        /// Build a mutable query that can be used to perform lookups with the
        /// supplied instance of <see cref="Type:DBLookupManager"/>.
        /// </summary>
        /// <returns>A newly-constructed mutable instance of <see cref="Type:DBLookupBase"/>
        /// that can be used to perform lookup operations with the specified <see cref="Type:DBLookupManager"/> instance.</returns>
        /// <param name="mgr">Instance which will execute the lookup that is returned by this method</param>
        /// <param name="query">(Optional, default null) Provides the key necessary for lookup operations</param>
        /// <seealso cref="DBLookupFactory.BuildLookup(DBLookupManager, ILookup)"/>
        /// <exception cref="ArgumentNullException"><c>mgr</c> is <c>null</c>.</exception>
        public static MutableDBLookup BuildMutableLookup(DBLookupManager mgr, ILookup query = null)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            if (query == null)
                return new MutableDBLookup(null, mgr.connection);

            //if (query is MutableDBLookup int_mutable_query)
                //return BuildMutableLookup(mgr, int_mutable_query.Unwrap_Immutable);
            if (query is IMutableLookup<ILookup> ext_mutable_query)
            {
                return BuildMutableLookup(mgr, new Lookup(ext_mutable_query.ImmutableCopy().KeyCopy));
            }

            // Avoid an extra copy operation when working with internally-defined types
            if (query is Lookup int_query)
                return new MutableDBLookup(int_query.Key_Internal);
            return new MutableDBLookup(query.KeyCopy, mgr.connection);
        }

        /// <summary>
        /// Build an immutable query that can be used to perform lookups with the
        /// supplied instance of <see cref="Type:DBLookupManager"/>.
        /// </summary>
        /// <returns>A newly-constructed immutable instance of <see cref="Type:DBLookupBase"/>
        /// that can be used to perform lookup operations with the specified <see cref="Type:DBLookupManager"/> instance</returns>
        /// <param name="mgr">(NOT NULL) Instance which will execute the lookup that is returned by this method</param>
        /// <param name="query">(NOT NULL) Provides the key necessary for lookup operations</param>
        /// <seealso cref="DBLookupFactory.BuildMutableLookup(DBLookupManager, ILookup)"/>
        /// <exception cref="ArgumentNullException"><c>mgr</c> or <c>query</c> is <c>null</c>.</exception>
        public static DBLookupBase BuildLookup(DBLookupManager mgr, ILookup query)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return DBLookup.Build(mgr, query);
        }

        /// <summary>
        /// Builds a result of a lookup operation. All lookup results are immutable.
        /// </summary>
        /// <returns>
        /// A newly-constructed interface to an object representing the
        /// result of a lookup operation.
        /// </returns>
        /// <param name="query">(NOT NULL) Query.</param>
        /// <param name="result">(nullable) Result.</param>
        /// <exception cref="ArgumentNullException"><c>query</c> is <c>null</c>.</exception>
        internal static DBLookupResult BuildResult(DBLookup query, object result)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return DBLookupResult.Build_Internal(query, result);
        }

        /// <summary>
        /// Builds a result of a lookup operation. All lookup results are immutable.
        /// </summary>
        /// <returns>
        /// A newly-constructed interface to an object representing the
        /// result of a lookup operation.
        /// </returns>
        /// <param name="mgr">(NOT NULL) Manager responsible for the query</param>
        /// <param name="query">(NOT NULL) Object that knows how to provide the lookup key</param>
        /// <param name="response">(nullable) Response to the query.</param>
        /// <exception cref="ArgumentNullException">One or more required arguments are <c>null</c>.</exception>
        public static ILookupResult<DBLookupBase> BuildResult(DBLookupManager mgr, ILookup query, object response)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return DBLookupResult.Build(mgr, query, response);
        }
    }
}
