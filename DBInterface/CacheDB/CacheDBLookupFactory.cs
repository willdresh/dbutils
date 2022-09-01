// // CacheDBLookupFactory.cs
// // Will Dresh
// // w@dresh.app
using System;
using MutableVerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;

namespace DBInterface.CacheDB
{
    /// <summary>
    /// Simple factory class to provide <see cref="ICacheLookup"/> instances for
    /// use with <see cref="T:CacheDBLookupManager"/> 
    /// </summary>
    public static partial class CacheDBLookupFactory
    {
        /// <summary>
        /// Builds the lookup.
        /// </summary>
        /// <returns>The lookup.</returns>
        /// <param name="manager">Manager.</param>
        /// <param name="lookup">Lookup.</param>
        /// <param name="bypassCache">If set to <c>true</c> bypass cache.</param>
        /// <param name="dontCacheResult">If set to <c>true</c> dont cache result.</param>
        /// <exception cref="CustomTypeFailedVerificationException">A custom implementation of IMutableLookup failed behavioral verification</exception>
        public static ICacheLookup BuildLookup
        (CacheDBLookupManager manager, ILookup lookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (lookup == null) throw new ArgumentNullException(nameof(lookup));


            // Unwrap mutable DBLookupBase
            // U
            if (lookup is IMutableLookup<DBLookupBase> mdblb)
            {
                DBLookupBase copy = mdblb.ImmutableCopy();
                if (copy is DBLookup int_dbl)
                    return BuildLookup(manager, int_dbl, bypassCache, dontCacheResult);

                return BuildLookup(manager, copy, bypassCache, dontCacheResult);
            }

            // Try to unwrap mutable any-type (triggers verification of mutability behavior)
            // If verification success, then recurse by passing an immutable copy
            // If verification fails, throw exception.
            if (lookup is IMutableLookup<ILookup> iml)
            {
                ILookup copy = iml.ImmutableCopy();

                // Verification Trigger
                if (!LookupManager.VerifyInstance(iml, out MutableVerificationFlags flags))
                    throw new CustomTypeFailedVerificationException(flags);

                return BuildLookup(manager, copy, bypassCache, dontCacheResult);
            }

            if (lookup is DBLookup dbl) // Using a more-derived type, when possible, will prevent extraneous copy operations
                return new CacheDBLookup(dbl, bypassCache, dontCacheResult);
            if (lookup is DBLookupBase dblb)
                return new CacheDBLookup(dblb, bypassCache, dontCacheResult);

            return new CacheDBLookup(DBLookup.Build(manager,lookup), bypassCache, dontCacheResult);
        }

        public static MutableCacheDBLookup BuildMutableLookup(CacheDBLookupManager manager, ILookup lookup = null, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            return MutableCacheDBLookup.Build_Internal(manager, lookup, bypassCache, dontCacheResult);
        }

        internal static CacheDBLookup BuildLookup_Internal(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (dbLookup == null) throw new ArgumentNullException(nameof(dbLookup));
            return CacheDBLookup.Build_Internal(dbLookup, bypassCache, dontCacheResult);
        }

        public static MutableCacheDBLookup BuildMutableLookup(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (dbLookup == null) throw new ArgumentNullException(nameof(dbLookup));
            return MutableCacheDBLookup.Build_Mutable_Copy_Internal(dbLookup, bypassCache, dontCacheResult);
        }
    }
}
