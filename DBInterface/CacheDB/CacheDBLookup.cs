// CacheDBLookup.cs
// Will Dresh
// w@dresh.app

using System;
using System.Data;

namespace DBInterface.CacheDB
{
    public enum DataSource { CACHE, DATABASE, NONE }

    public interface ICacheLookupResult: ILookupResult<ICacheLookup>, ILookupResult<ILookup>
    {
        DataSource ActualSource { get; }
    }

    /// <summary>
    /// CacheDB Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal class CacheDBLookup: DBLookup, ICacheLookup,
        IEquatable<CacheDBLookup>, IEquatable<ICacheLookup>
    {
        internal sealed class CacheDBLookupBugDetectedException: ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of CacheDBLookup; this probably means a bug in the caller's code";
            internal CacheDBLookupBugDetectedException(string argumentName)
                :base(argumentName, FBDEMessage) {  }
        }

        internal CacheDBLookup(DBLookup dbLookup, bool bypassCache, bool dontCacheResult)
            : base(dbLookup.ReadOnlyKey, dbLookup.DBConnection)
        {
            BypassCache = bypassCache;
            DontCacheResult = dontCacheResult;
        }

        internal CacheDBLookup(DBLookupBase ext_dbLookup, bool bypassCache, bool dontCacheResult)
            : base(ext_dbLookup.KeyCopy, ext_dbLookup.DBConnection)
        {
            BypassCache = bypassCache;
            DontCacheResult = dontCacheResult;
        }

        internal CacheDBLookup(MutableCacheDBLookup mcdl)
            :base(mcdl.Key_Internal, mcdl.DBConnection)
        {
            BypassCache = mcdl.BypassCache;
            DontCacheResult = mcdl.DontCacheResult;
        }

        internal CacheDBLookup(string key, IDbConnection dbConnection, bool bypassCache, bool dontCacheResult)
            :base(key, dbConnection)
        {
            BypassCache = bypassCache;
            DontCacheResult = dontCacheResult;
        }

        public bool BypassCache { get; internal set; }
        public bool DontCacheResult { get; internal set; }

        ///<summary>Build a lookup (internal use only).</summary>
        /// <exception cref="CacheDBLookupBugDetectedException"><c>dbLookup</c> is <c>null</c></exception>
        internal static CacheDBLookup Build_Internal(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (dbLookup == null)
                throw new CacheDBLookupBugDetectedException(nameof(dbLookup));

            return new CacheDBLookup(dbLookup, bypassCache, dontCacheResult);
        }

        ///<summary>Build a lookup (internal use only).</summary>
        /// <exception cref="CacheDBLookupBugDetectedException"><c>mgr</c> is <c>null</c></exception>
        internal static CacheDBLookup Build_Internal(DBLookupManager mgr, ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (mgr == null)
                throw new CacheDBLookupBugDetectedException(nameof(mgr));

            return new CacheDBLookup((query is DBLookupBase dbLookupBase) ? dbLookupBase.ReadOnlyKey : query.KeyCopy, 
                mgr.connection, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Build a lookup 
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> is <c>null</c>,</exception>
        public static CacheDBLookup Build(DBLookupManager mgr, ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            return Build_Internal(mgr, query, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">(nullable)</param>
        /// <param name="dbLookup">(NOT NULL)</param>
        /// <param name="cacheLookup">(NOT NULL)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> or <c>cacheLookup</c> is <c>null</c></exception>
        public static CacheDBLookup BuildUnion(string key, DBLookupBase dbLookup, ICacheLookup cacheLookup)
        {
            if (dbLookup == null)
                throw new ArgumentNullException(nameof(dbLookup));
            if (cacheLookup == null)
                throw new ArgumentNullException(nameof(cacheLookup));


            return new CacheDBLookup(key, dbLookup.DBConnection, cacheLookup.BypassCache, cacheLookup.DontCacheResult);
        }

        /// <summary>
        /// Build a lookup 
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> is <c>null</c>,</exception>
        public static CacheDBLookup Build(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (dbLookup == null)
                throw new ArgumentNullException(nameof(dbLookup));

            return Build_Internal(dbLookup, bypassCache, dontCacheResult);
        }

        public bool Equals(CacheDBLookup other)
        {
            return base.Equals(other) &&
                other.BypassCache == BypassCache &&
                other.DontCacheResult == DontCacheResult;
        }

        public bool Equals(ICacheLookup other)
        {
            if (other is IMutableLookup<ICacheLookup> ext_mu_icl) {
                if (!LookupManager.VerifyInstance(ext_mu_icl as IMutableLookup<ILookup>, out LookupManager.External_IMutableLookup_VerificationFlags flags))
                    throw new LookupManager.CustomTypeFailedVerificationException(flags);

                return Equals(ext_mu_icl.ImmutableCopy());
            }

            return base.Equals(other) &&
                other.BypassCache == BypassCache &&
                other.DontCacheResult == DontCacheResult;
        }
    }


    /// <summary>
    /// CacheDB Lookup result. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    public sealed class CacheDBLookupResult: ILookupResult<ILookup>, ILookupResult<ICacheLookup>
    {
        internal CacheDBLookupResult(ICacheLookup query, object response, DataSource src)
        { ActualSource = src; Query = query;  Response = response; }

        public DataSource ActualSource { get; }
        public object Response { get; }
        public ICacheLookup Query { get; }
        ILookup ILookupResult<ILookup>.Query { get { return Query; } }

        internal static CacheDBLookupResult Build_Internal(ICacheLookup query, object response, DataSource src)
        {
            return new CacheDBLookupResult(query, response, src);
        }

        public static ILookupResult<ICacheLookup> Build(ICacheLookup query, object response, DataSource src)
        {
            return Build_Internal(query, response, src);
        }
    }
}
