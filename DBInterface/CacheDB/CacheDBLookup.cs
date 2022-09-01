// // CacheLookup.cs
// // Will Dresh
// // w@dresh.app

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
    internal class CacheDBLookup: DBLookup, ICacheLookup, IEquatable<CacheDBLookup>
    {
        internal sealed class CacheDBLookupBugDetectedException: ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of CacheDBLookup; this probably means a bug in the caller's code";
            internal CacheDBLookupBugDetectedException(string argumentName)
                :base(argumentName, FBDEMessage) {  }
        }

        private const int Hashcode_Operand = 20;
        private const int Hashcode_XOR_Operand_BypassCache = 4;
        private const int Hashcode_XOR_Operand_DontCacheResult = 8;

        internal CacheDBLookup(DBLookup dbLookup, bool bypassCache, bool dontCacheResult)
            : base(dbLookup.Key_Internal, dbLookup.DBConnection)
        {
            BypassCache = bypassCache;
            DontCacheResult = dontCacheResult;
        }

        internal CacheDBLookup(DBLookupBase dbLookup, bool bypassCache, bool dontCacheResult)
            : base(dbLookup.KeyCopy, dbLookup.DBConnection)
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

            if (query is DBLookupBase dBLookupBase)
                return new CacheDBLookup(dBLookupBase.Key_Internal, mgr.connection, bypassCache, dontCacheResult);

            return new CacheDBLookup(query.KeyCopy, mgr.connection, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Build a lookup 
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> is <c>null</c>,</exception>
        public static ICacheLookup Build(DBLookupManager mgr, ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            return Build_Internal(mgr, query, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Build a lookup 
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> is <c>null</c>,</exception>
        public static ICacheLookup Build(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
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

        public override bool Equals(object obj)
        {
            if (obj is CacheDBLookup)
                return Equals(obj as CacheDBLookup);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ (BypassCache ? Hashcode_XOR_Operand_BypassCache : 0)
                ^ (DontCacheResult ? Hashcode_XOR_Operand_DontCacheResult : 0)
                ^ Hashcode_Operand;
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
