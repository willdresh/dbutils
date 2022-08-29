// // CacheDBLookupProvider.cs
// // Will Dresh
// // w@dresh.app


using System;
using CacheManager.Core;

namespace DBInterface.Cache
{
        public class IllegalCacheOperationException : ApplicationException
        { }

        public sealed class CacheFunctionProvider
        {
            public sealed class NullKeyException : IllegalCacheOperationException
            { }

            public sealed class NullQueryException : IllegalCacheOperationException
            { }

            private readonly ICacheManager<Object> cacheManager;

            public CacheFunctionProvider(ICacheManager<object> _cacheManager)
            {
                cacheManager = _cacheManager ?? throw new ArgumentNullException(nameof(_cacheManager));
            }

            private void ValidateParams(string key)
            {
                if (key == null) throw new NullKeyException();
            }

            private void ValidateParams(ILookup query)
            {
                if (query == null) throw new NullQueryException();
                ValidateParams(query.Key);
            }

            private void ValidateParams(ILookupResult entry)
            {
                if (entry == null) throw new ArgumentNullException(nameof(entry));
                ValidateParams(entry.Query);
            }

            //public class NullResultException: IllegalCacheOperationException
            //{ }

            /// <summary>
            /// Lookup the specified query using the supplied cache manager instance
            /// </summary>
            /// <returns>The lookup (if cache hit); <c>null</c> if cache miss.</returns>
            /// <param name="query">Query.</param>
            /// <exception cref="System.ArgumentNullException">One or more parameters are null</exception>
            /// <exception cref="NullKeyException">The supplied query has a null key</exception>
            public ILookupResult CacheLookup(ILookup query)
            {
                ValidateParams(query);
                return new LookupResult(query, cacheManager.Get(query.Key));
            }

            public ILookupResult CacheLookup(string key)
            {
                return CacheLookup(new Lookup(key));
            }

            /// <summary>
            /// Check whether cache contains an entry for the desired query
            /// </summary>
            /// <returns><c>true</c>, if cache contains specified entry, <c>false</c> otherwise.</returns>
            /// <param name="query">Query.</param>
            /// <exception cref="System.ArgumentNullException">One or more parameters are null</exception>
            /// <exception cref="NullKeyException"><c>query</c> has a null key</exception>
            public bool CacheContains(ILookup query)
            {
                ValidateParams(query);

                return CacheContains(query.Key);
            }

            public bool CacheContains(string key)
            {
                return cacheManager.Exists(key);
            }

            /// <summary>
            /// Save data to the cache
            /// </summary>
            /// <param name="entry">Entry.</param>
            /// <exception cref="System.ArgumentNullException">One or more parameters are null</exception>
            /// <exception cref="IllegalCacheOperationException">One or more required properties or
            /// sub-properties of <c>entry</c>is null</exception>
            public void CacheSave(ILookupResult entry)
            {
                ValidateParams(entry);
                cacheManager.Put(entry.Query.Key, entry.Response);
            }

            public void CacheSave(string key, object value)
            {
                ValidateParams(key);
                cacheManager.Put(key, value);
            }
        }
    }
