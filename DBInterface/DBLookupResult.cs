// // DBLookupResult.cs
// // Will Dresh
// // w@dresh.app
using System;
namespace DBInterface
{
    /// <summary>
    /// DB Lookup result.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no public constructors.
    /// </remarks>
    internal partial class DBLookupResult : LookupResult, ILookupResult<DBLookupBase>
    {
        internal DBLookupResult(DBLookupBase query, object response)
            : base(query, response)
        { }

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
        internal static DBLookupResult Build_Internal(DBLookupBase query, object response)
        {
            if (query == null)
                throw new DBLookupResult_BugDetectedException(nameof(query));

            return new DBLookupResult(query, response);
        }
    }
}
