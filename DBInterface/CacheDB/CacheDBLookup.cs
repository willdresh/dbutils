// // CacheLookup.cs
// // Will Dresh
// // w@dresh.app

using System;
using System.Data;

namespace DBInterface.CacheDB
{
    public enum DataSource { CACHE, DATABASE, NONE }

    public interface ICacheDBLookup : ILookup
    {
        bool BypassCache { get; }
        bool DontCacheResult { get; }
    }

    public interface ICacheDBLookupResult: ILookupResult<ICacheDBLookup>
    {
        DataSource ActualSource { get; }
    }

    /// <summary>
    /// Cache-DB Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal class CacheDBLookup: DBLookup, ICacheDBLookup, IEquatable<CacheDBLookup>
    {
        private const int Hashcode_Operand = 20;
        private const int Hashcode_XOR_Operand_BypassCache = 4;
        private const int Hashcode_XOR_Operand_DontCacheResult = 8;

        internal CacheDBLookup(MutableCacheDBLookup mcdl)
            :base(mcdl.Key, mcdl.DBConnection)
        {
            BypassCache = mcdl.BypassCache;
            DontCacheResult = mcdl.DontCacheResult;
        }

        internal CacheDBLookup(string key, IDbConnection dbConnection, bool bypassCache = false, bool dontCacheResult = false)
            :base(key, dbConnection)
        {
            BypassCache = bypassCache;
            DontCacheResult = dontCacheResult;
        }

        public bool BypassCache { get; internal set; }
        public bool DontCacheResult { get; internal set; }

        internal static CacheDBLookup Build_Internal(ILookup query, CacheDBLookupManager mgr, bool bypassCache = false, bool dontCacheResult = false)
        {
            return new CacheDBLookup(query.Key, mgr.connection, bypassCache, dontCacheResult);
        }

        public static ICacheDBLookup Build(ILookup query, CacheDBLookupManager mgr, bool bypassCache = false, bool dontCacheResult = false)
        {
            return Build_Internal(query, mgr, bypassCache, dontCacheResult);
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

    public sealed class MutableCacheDBLookup: ICacheDBLookup, IMutableLookup<ICacheDBLookup>
    {
        private CacheDBLookup cdlu;

        private MutableCacheDBLookup(ILookup _lu, CacheDBLookupManager manager, bool bypassCache, bool dontCacheResult)
        {
            cdlu = new CacheDBLookup(_lu.Key, manager.connection, bypassCache, dontCacheResult);
        }

        internal MutableCacheDBLookup(string key = null, IDbConnection dbConnection = null, bool bypassCache = false, bool dontCacheResult = false)
        {
            cdlu = new CacheDBLookup(key, dbConnection, bypassCache, dontCacheResult);
        }

        public bool BypassCache { get => cdlu.BypassCache; set => cdlu.BypassCache = value; }
        public bool DontCacheResult { get => cdlu.BypassCache; set => cdlu.DontCacheResult = value; }
        internal IDbConnection DBConnection { get => cdlu.DBConnection; }
        public string Key { get => cdlu.Key; set => cdlu.Key = value; }

        internal static MutableCacheDBLookup Build_Internal(DBLookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            return new MutableCacheDBLookup(query.Key, query.DBConnection, bypassCache, dontCacheResult);
        }

        internal static MutableCacheDBLookup Build_Internal(ILookup lu, CacheDBLookupManager manager, bool bypassCache = false, bool dontCacheResult = false)
        {
            return new MutableCacheDBLookup(lu, manager, bypassCache, dontCacheResult);
        }

        public static IMutableLookup<ICacheDBLookup> Build(ILookup lu, CacheDBLookupManager manager, bool bypassCache = false, bool dontCacheResult = false)
        {
            return Build_Internal(lu, manager, bypassCache, dontCacheResult);
        }

        internal CacheDBLookup AsImmutable_Internal()
        {
            return cdlu;
        }

        public ICacheDBLookup AsImmutable()
        {
            return AsImmutable_Internal();
        }
    }

    internal class CacheDBLookupResult: LookupResult, ILookupResult<CacheDBLookup>
    {
        internal CacheDBLookupResult(CacheDBLookup query, object response, DataSource src)
            : base(query, response)
        {
            ActualSource = src;
        }

        internal DataSource ActualSource { get; }
        public new CacheDBLookup Query { get { return base.Query as CacheDBLookup; } }

        internal static CacheDBLookupResult Build(ILookupResult<DBLookup> result, DataSource src)
        {
            CacheDBLookup query = result.Query as CacheDBLookup;
            if (query == null)
                query = CacheDBLookup.Build(result.Query) as CacheDBLookup;
            return new CacheDBLookupResult(query, result.Response, src);
        }
    }
}