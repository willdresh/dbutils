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
    internal class DBLookup: DBLookupBase
    {
        internal sealed class DBLookupBugDetectedException : ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of DBLookup; this probably means a bug in the caller's code";
            public DBLookupBugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }

        internal DBLookup(string key, System.Data.IDbConnection dbConnection)
            : base(key, dbConnection)
        { }

        internal DBLookup(MutableDBLookup other)
            : base(other.Key, other.DBConnection)
        {  }

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

            return new DBLookup(query?.Key, mgr.connection);
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

            return new DBLookup(other.Key, other.DBConnection);
        }
    }

    /// <summary>
    /// Mutable wrapper for <see cref="Type:DBLookup"/>. This class cannot be
    /// externally inherited, as it has no public constructors.
    /// </summary>
    internal sealed class MutableDBLookup: IMutableLookup<DBLookupBase>
    {
        private DBLookupBase dbl;

        internal MutableDBLookup(string key = null, System.Data.IDbConnection dbConnection = null)
        {
            dbl = new DBLookup(key, dbConnection);
        }

        // DBConnection must never be made public to ensure integrity!
        internal IDbConnection DBConnection { get => dbl.DBConnection; set => dbl.DBConnection = value; }

        public string Key => dbl.Key;

        /// <summary>
        /// Get an immutable copy of this mutable object
        /// </summary>
        /// <returns>A newly-constructed immutable copy of <c>this</c>.</returns>
        public DBLookupBase ImmutableCopy()
        {
            return new DBLookup(this);
        }
    }

    /// <summary>
    /// DB Lookup result.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no public constructors.
    /// </remarks>
    internal class DBLookupResult: LookupResult, ILookupResult<DBLookupBase>
    {

        /// <summary>
        /// DBLookupResult bug detected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="ArgumentNullException"/>.
        /// </remarks>
        internal sealed class DBLookupResult_BugDetectedException : ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of DBLookupResult; this probably means a bug in the caller's code";
            public DBLookupResult_BugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }

        /// <summary>
        /// Internal instance expected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="SecurityException"/>
        /// </remarks>
        internal sealed class InternalInstanceExpectedException : SecurityException
        {
            private static readonly string IIEEMessage = "Internal DBLookupResult instance expected";

            internal InternalInstanceExpectedException()
                :base(IIEEMessage) {  }

            internal InternalInstanceExpectedException(string message)
                : base(GenerateMessage(message)) { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", IIEEMessage, msg);
            }
        }

        internal DBLookupResult(DBLookup query, object response)
            : base(query, response)
        { }

        internal DBLookup Query_Internal { get { return base.Query as DBLookup; } }
        public new DBLookupBase Query { get { return base.Query as DBLookupBase; } }


        /// <summary>
        /// Factory method
        /// </summary>
        /// <returns>A new instance</returns>
        /// <param name="manager">(NOT NULL) Manager.</param>
        /// <param name="query">(NOT NULL) Query.</param>
        /// <param name="response">(nullable) Response.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <c>manager</c> is <c>null</c>, <c>query</c> is <c>null</c>,
        ///     or both <c>manager</c> and <c>query</c> are <c>null</c>.
        /// </exception>
        public static ILookupResult<DBLookupBase> Build(DBLookupManager manager, ILookup query, object response)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return new DBLookupResult(DBLookup.Build(manager, query), response);
        }

        /// <summary>
        /// Builds the internal.
        /// </summary>
        /// <returns>A new instance</returns>
        /// <param name="query">(NOT NULL) Query.</param>
        /// <param name="response">(nullable) Response.</param>
        /// <exception cref="DBLookupResult_BugDetectedException"><c>query</c> is <c>null</c>.</exception>
        /// <exception cref="InternalInstanceExpectedException"><c>query</c> is NOT an instance of <see cref="DBLookup"/>.
        /// this is probably because a</exception>
        internal static DBLookupResult Build_Internal(DBLookupBase query, object response)
        {
            if (query == null)
                throw new DBLookupResult_BugDetectedException(nameof(query));

            if (query is DBLookup immutableQuery)
                return new DBLookupResult(immutableQuery, response);

            throw new InternalInstanceExpectedException();
        }
    }


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
        public static IMutableLookup<DBLookupBase> BuildMutableLookup(DBLookupManager mgr, ILookup query = null)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            return new MutableDBLookup(query?.Key, mgr.connection);
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
        internal static ILookupResult<DBLookupBase> BuildResult(DBLookupBase query, object result)
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
        internal static ILookupResult<DBLookupBase> BuildResult(DBLookupManager mgr, ILookup query, object response)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return DBLookupResult.Build(mgr, query, response);
        }
    }
}
