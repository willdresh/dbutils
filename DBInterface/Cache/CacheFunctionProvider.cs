// // CacheDBLookupProvider.cs
// // Will Dresh
// // w@dresh.app


using System;
using CacheManager.Core;

namespace DBInterface.Cache
{
        public class IllegalCacheOperationException : ApplicationException
        { }

        public sealed class CacheFunctionProvider: ILookupProvider<ILookup>
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
                if (query is Lookup int_query)
                    ValidateParams(int_query.Key_Internal);
                else ValidateParams(query.KeyCopy);
            }

            private void ValidateParams(ILookupResult<ILookup> entry)
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
            public ILookupResult<ILookup> CacheLookup(ILookup query)
            {
                ValidateParams(query);
                if (query is Lookup int_query)
                    return new LookupResult(query, cacheManager.Get(int_query.Key_Internal));

                return new LookupResult(query, cacheManager.Get(query.KeyCopy));
            }

            public ILookupResult<ILookup> CacheLookup(string key)
            {
                return CacheLookup(new Lookup(key));
            }

            internal bool CacheContains_Internal(Lookup query)
            {
                ValidateParams(query);
                return CacheContains(query.Key_Internal);
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
                if (query is Lookup int_query) return CacheContains_Internal(int_query);
                ValidateParams(query);
                return CacheContains(query.KeyCopy);
            }

            internal bool CacheContains(string key)
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
            public void CacheSave(ILookupResult<ILookup> entry)
            {
                ValidateParams(entry);
            ILookup query = entry.Query;
            if (query is Lookup int_query)
                cacheManager.Put(int_query.Key_Internal, entry.Response);
            else
                cacheManager.Put(query.KeyCopy, entry.Response);
            }

            internal void CacheSave(string key, object value)
            {
                cacheManager.Put(key, value);
            }

        public ILookupResult<ILookup> Lookup(ILookup query)
        {
            return CacheLookup(query);
        }
    }
}
