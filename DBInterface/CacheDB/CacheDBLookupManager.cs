// // CacheDBLookupProvider.cs
// // Will Dresh
// // w@dresh.app

// Vim marks: a= Lookup Method Implementation

using System;
using System.Data;
using DBInterface.Cache;
using CacheManager.Core;

namespace DBInterface.CacheDB
{

    public class CacheInsertNotAllowedException: ApplicationException
    { }

    public class CacheLookupNotAllowedException: ApplicationException
    { }

    public partial class CacheDBLookupManager: DBLookupManager, ILookupProvider
    {
        [Flags]
        public enum CachePolicy
        {
            PREFER_CACHE = 1,
            ALLOW_CUSTOM_CACHE_INSERT = 2,
            ALLOW_CUSTOM_CACHE_LOOKUP = 4
        }

        public const CachePolicy DEFAULT_Policy =
            CachePolicy.PREFER_CACHE | CachePolicy.ALLOW_CUSTOM_CACHE_INSERT | CachePolicy.ALLOW_CUSTOM_CACHE_LOOKUP;

        private readonly CacheFunctionProvider clp;

        /// <summary>
        /// Gets the cache policy.
        /// </summary>
        /// <value>The cache policy.</value>
        public CachePolicy Policy { get; }

        public CacheDBLookupManager(IDbConnection connection, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            :base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(isBackplaneSource));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, string runtimeCacheHandleName, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(runtimeCacheHandleName, isBackplaneSource));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, ExpirationMode expirationMode, TimeSpan ttl, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(isBackplaneSource)
                .WithExpiration(expirationMode, ttl));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, string runtimeCacheHandleName, ExpirationMode expirationMode, TimeSpan ttl, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(runtimeCacheHandleName, isBackplaneSource)
                .WithExpiration(expirationMode, ttl));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, ICacheManager<Object> _cacheManager, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy)
            : base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            clp = new CacheFunctionProvider(_cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, Action<ConfigurationBuilderCachePart> cacheManagerSettings, CachePolicy _cachePolicy = DEFAULT_Policy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy)
            : base(connection, connectionPolicy)
        {
            Policy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(cacheManagerSettings);
            clp = new CacheFunctionProvider(cacheManager);
        }

        private void CacheUpdate(CacheDBLookupResult clr)
        {
            if (clr == null)
                throw new ArgumentNullException(nameof(clr), "Result must not be null in order to update cache");

            if (!clr.Query.DontCacheResult)
                clp.CacheSave(clr);
        }

        internal static CacheDBLookupResult GetFailureInstance_Internal(CacheDBLookup query)
        {
            return new CacheDBLookupResult(query, null, DataSource.NONE);
        }

        public static ILookupResult<CacheDBLookup> GetFailureInstance(CacheDBLookup query)
        {
            return GetFailureInstance_Internal(query);
        }

        private CacheDBLookupResult CacheLookup(CacheDBLookup query)
        {
            if (query.BypassCache || !clp.CacheContains(query))
                return GetFailureInstance_Internal(query);

            else
                return new CacheDBLookupResult(query, clp.CacheLookup(query), DataSource.CACHE);
        }

        internal CacheDBLookup BuildLookup_Internal(ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            return CacheDBLookup.Build_Internal(query, this, bypassCache, dontCacheResult);
        }

        public ICacheDBLookup BuildLookup(ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            return BuildLookup_Internal(query, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Lookup the specified query, according to policy
        /// </summary>
        /// <returns>The lookup result, having been retrieved either from the cache
        /// or from the database. If no result was found, returns null.</returns>
        /// <param name="query">Query.</param>
        internal virtual CacheDBLookupResult Lookup(CacheDBLookup query)
        {
            CacheDBLookupResult result = null;
            bool cacheFirst = Policy.HasFlag(CachePolicy.PREFER_CACHE);

            if (cacheFirst && clp.CacheContains(query))
                result = CacheLookup(query);
            else
            {
                bool gotFromDatabase = false;
                try
                {
                    DBLookupResult dbResult = DatabaseLookup(query);
                    result = CacheDBLookupResult.Build(dbResult, DataSource.DATABASE);
                    gotFromDatabase = true;
                    CacheUpdate(result);
                }
                catch (PolicyProhibitsAutoConnectException)
                {
                    result = GetFailureInstance_Internal(query);
                }

                if (!cacheFirst && !gotFromDatabase)
                    result = CacheLookup(query);
            }

            return result;
        }

        ILookupResult ILookupProvider<ILookup>.Lookup(ILookup query)
        {
            return Lookup(BuildLookup(query));
        }

        public ILookupResult Lookup(ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            return Lookup(BuildLookup(query, bypassCache, dontCacheResult));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.Cache.CacheDBLookupManager"/> can custom cache lookup.
        /// </summary>
        /// <value><c>true</c> if can custom cache lookup; otherwise, <c>false</c>.</value>
        public bool CanCustomCacheLookup { get { return Policy.HasFlag(CachePolicy.ALLOW_CUSTOM_CACHE_LOOKUP); } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.Cache.CacheDBLookupManager"/> can custom cache insert.
        /// </summary>
        /// <value><c>true</c> if can custom cache insert; otherwise, <c>false</c>.</value>
        public bool CanCustomCacheInsert { get { return Policy.HasFlag(CachePolicy.ALLOW_CUSTOM_CACHE_INSERT); } }

        /// <summary>
        /// Lookups the cache only.
        /// </summary>
        /// <returns>The cache only.</returns>
        /// <param name="key">Key.</param>
        /// <exception cref="CacheLookupNotAllowedException">thrown if policy prohibits the
        /// use of custom cache lookups</exception>
        public object Lookup_CacheOnly(string key)
        {
            if (CanCustomCacheLookup)
                return clp.CacheLookup(key);
            else throw new CacheLookupNotAllowedException();
        }

        /// <summary>
        /// Lookups the cache only.
        /// </summary>
        /// <returns>The cache only.</returns>
        /// <param name="lookup">Lookup.</param>
        /// <exception cref="CacheLookupNotAllowedException">thrown if policy prohibits the
        /// use of custom cache lookups</exception>
        public ILookupResult Lookup_CacheOnly(ILookup lookup)
        {
            if (CanCustomCacheLookup)
                return clp.CacheLookup(lookup);
            else throw new CacheLookupNotAllowedException();
        }

        /// <summary>
        /// Inserts the cache only.
        /// </summary>
        /// <param name="lookupResult">Lookup result.</param>
        /// <exception cref="CacheInsertNotAllowedException">thrown if policy prohibits the
        /// use of custom cache insertions</exception>
        public void Insert_CacheOnly(ILookupResult lookupResult)
        {
            if (CanCustomCacheInsert)
                clp.CacheSave(lookupResult);
            else throw new CacheInsertNotAllowedException();
        }

        /// <summary>
        /// Inserts the cache only.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <exception cref="CacheInsertNotAllowedException">thrown if policy prohibits the
        /// use of custom cache insertions</exception>
        public void Insert_CacheOnly(string key, object value)
        {
            if (CanCustomCacheInsert)
                clp.CacheSave(key, value);
            else throw new CacheInsertNotAllowedException();
        }
    }
}
