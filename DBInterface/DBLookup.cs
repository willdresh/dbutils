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

    /// <summary>
    /// Mutable wrapper for <see cref="DBLookup"/>. This class cannot be
    /// externally inherited, as it has no public constructors.
    /// </summary>
    public class MutableDBLookup: IMutableLookup<DBLookupBase>, IMutableLookup<ILookup>
    {
        private DBLookup dbl;

        internal MutableDBLookup(string key = null, System.Data.IDbConnection dbConnection = null)
        {
            dbl = new DBLookup(key, dbConnection);
        }

        // DBConnection must never be made public to ensure integrity!
        private IDbConnection DBConnection
        {
            get => dbl.DBConnection;
            set => dbl.DBConnection = value;
        }
	
    	internal DBLookup Unwrap_Immutable { get => dbl; }
    	internal string Key_Internal { get => dbl.Key_Internal; set => dbl.Key_Internal = value; }
    	public string KeyCopy { get => dbl.KeyCopy; set => dbl.KeyCopy = value; }

        internal DBLookupBase ImmutableCopy_Internal()
        {
            return new DBLookup(this);
        }

        DBLookupBase IMutableLookup<DBLookupBase>.ImmutableCopy()
        {
            return ImmutableCopy_Internal();
        }

        public ILookup ImmutableCopy()
        {
            return ImmutableCopy_Internal();
        }

        public bool Equals(DBLookupBase dblb)
    	{
    		return ReferenceEquals(DBConnection, dblb.DBConnection)
    			&& (this.Key_Internal == null ? other.Key_Internal == null : this.Key_Internal.Equals(other.Key_Internal));
    	}

        public bool Equals(ILookup other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is MutableDBLookup int_mdbl) 
		return Equals(int_mdbl.Unwrap_Immutable as DBLookupBase);
	    if (other is DBLookupBase int_dblb)
                return Equals(int_dblb);
	    else if (other is IMutableLookup<DBLookupBase> ext_mdbl) {
		DBLookupBase copy = ext_mdbl.ImmutableCopy();
		return ReferenceEquals(DBConnection, copy.DBConnection)
			&& (this.Key_Internal == null ? copy.Key_Internal == null : this.Key_Internal.Equals(copy.Key_Internal));
	    }

	    return false;
        }
    }

    /// <summary>
    /// DB Lookup result.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no public constructors.
    /// </remarks>
    internal partial class DBLookupResult: LookupResult, ILookupResult<DBLookupBase>
    {
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
        /// Builds an instance from a query and response (internal use only)
        /// </summary>
        /// <returns>A new instance</returns>
        /// <param name="query">(NOT NULL) Query.</param>
        /// <param name="response">(nullable) Response.</param>
        /// <exception cref="DBLookupResult_BugDetectedException"><c>query</c> is <c>null</c>.</exception>
        internal static DBLookupResult Build_Internal(DBLookup query, object response)
        {
            if (query == null)
                throw new DBLookupResult_BugDetectedException(nameof(query));

            return new DBLookupResult(query, response);
        }

        internal static DBLookupResult Build_Internal(DBLookupBase query, DataTable result)
        {
            throw new NotImplementedException();
        }
    }
}
