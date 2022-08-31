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

    public partial class CacheDBLookupManager: DBLookupManager
    {
        [Flags]
        public enum CacheDBPolicy
        {
            PREFER_CACHE = 1,
            ALLOW_CUSTOM_CACHE_INSERT = 2,
            ALLOW_CUSTOM_CACHE_LOOKUP = 4
        }

        public const CacheDBPolicy DEFAULT_CacheDBPolicy =
            CacheDBPolicy.PREFER_CACHE | CacheDBPolicy.ALLOW_CUSTOM_CACHE_INSERT | CacheDBPolicy.ALLOW_CUSTOM_CACHE_LOOKUP;

        private readonly CacheFunctionProvider clp;

        /// <summary>
        /// Gets the cache policy.
        /// </summary>
        /// <value>The cache policy.</value>
        public CacheDBPolicy My_CacheDBPolicy { get; }

        public CacheDBLookupManager(IDbConnection connection, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            :base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(isBackplaneSource));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, string runtimeCacheHandleName, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(runtimeCacheHandleName, isBackplaneSource));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, ExpirationMode expirationMode, TimeSpan ttl, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(isBackplaneSource)
                .WithExpiration(expirationMode, ttl));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, string runtimeCacheHandleName, ExpirationMode expirationMode, TimeSpan ttl, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy, bool isBackplaneSource = false)
            : base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
            ICacheManager<Object> cacheManager = CacheFactory.Build<Object>(settings => settings
                .WithSystemRuntimeCacheHandle(runtimeCacheHandleName, isBackplaneSource)
                .WithExpiration(expirationMode, ttl));
            clp = new CacheFunctionProvider(cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, ICacheManager<Object> _cacheManager, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy)
            : base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
            clp = new CacheFunctionProvider(_cacheManager);
        }

        public CacheDBLookupManager(IDbConnection connection, Action<ConfigurationBuilderCachePart> cacheManagerSettings, CacheDBPolicy _cachePolicy = DEFAULT_CacheDBPolicy, DBConnectionPolicy connectionPolicy = DEFAULT_Connection_Policy)
            : base(connection, connectionPolicy)
        {
            My_CacheDBPolicy = _cachePolicy;
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

        /// <summary>
        /// When an external implementation of <see cref="IMutableCacheDBLookup"/>
        /// is passed to <see cref="CacheDBLookupManager.GetFailureInstance(ICacheDBLookup)"/>,
        ///  then a private verification check is performed.<br />
        /// The result of this (private) verification operation is an instance
        /// of <c>External_IMutableLookup_VerificationFlags</c>.
        /// </summary>
        [Flags]
        protected internal enum External_IMutableLookup_VerificationFlags {
            /// <summary>
            /// This flag indicates that the RTT of an IMutableCacheDBLookup
            /// is an internally-defined type. Accompanies the
            /// <see cref="External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED;"/> flag
            /// as the automatic result of submitting an expected internal instance for verification.
            /// </summary>
            /// <remarks>
            /// Only one or zero <c>INTERNAL_INSTANCE_*</c> flags should be set per verification;
            /// if more than one is set this may indicate an internal bug.
            /// </remarks>
            INTERNAL_INSTANCE_MCDL = 1,

            /// <summary>
            /// This flag indicates that the RTT of an IMutableCacheDBLookup
            /// is an internally-defined type. This flag SHOULD never get set,
            /// however this flag has been added for possible future use.<br />
            /// Presently, when this flag is set, the 
            /// <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE"/>
            /// flag will be set as well, although this behavior may change in
            /// future versions.
            /// </summary>
            /// <remarks>
            /// Only one or zero <c>INTERNAL_INSTANCE_*</c> flags should be set per verification;
            /// if more than one is set this may indicate an internal bug.
            /// </remarks>
            INTERNAL_INSTANCE_DBLB = 2,

            /// <summary>
            /// This flag indicates that the RTT of an IMutableCacheDBLookup
            /// is an internally-defined type. If this flag is set during a verification
            /// check, it could indicate an internal bug.<br />
            /// When this flag is set, the <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE"/>
            /// flag will be set as well.
            /// </summary>
            /// <remarks>
            /// Only one or zero <c>INTERNAL_INSTANCE_*</c> flags should be set per verification;
            /// if more than one is set this may indicate an internal bug.
            /// </remarks>
            INTERNAL_INSTANCE_MDBL = 4,

            /// <summary>
            /// This flag indicates whether <c>IMutableCacheDBLookup.ImmutableCopy()</c>
            /// has been called on an instance in order to test its integrity
            /// for use with <see cref=" CacheDBLookupManager"/>. This should be
            /// true for externally-defined types, and false for internally-defined ones.
            /// </summary>
            TESTED_RTT = 8,

            /// <summary>
            /// This flag indicates whether an <c>IMutableCacheDBLookup</c> instance
            /// succeeded verification for use with <see cref="CacheDBLookupManager"/>.
            /// </summary>
            TESTED_EQUALITY=16,

            /// <summary>
            /// This flag indicates that something unexpected occurred while trying
            /// to infer the RTT of an <c>IMutableCacheDBLookup</c> instance. When
            /// this flag is set, it may indicate a bug, or that <see cref="CacheDBLookupManager"/>
            /// is being used in a manner other than it was designed.
            /// </summary>
            RTT_TROUBLE = 32,

            /// <summary>
            /// This flag indicates that the equality methods for an instance of
            /// externally-defined <c>IMutableCacheDBLookup</c> instance have been
            /// found to exhibit noncommutative behavior, resulting in verification failure.
            /// </summary>
            ERR_EQUALITY_NONCOMMUTATIVE = 64,

            /// <summary>
            /// This flag indicates that the implementation of <c>IMutableCacheDBLookup.ImmutableCopy()</c>
            /// yielded a copy which was "not equal" to the original,
            /// resulting in verification failure.
            /// </summary>
            ERR_ORIGINAL_NOT_EQUALTO_COPY = 128,

            /// <summary>
            /// This flag indicates that the implementation of <c>IMutableCacheDBLookup.ImmutableCopy()</c>
            /// yielded a copy which was NOT actually immutable, resulting in verification failure
            /// </summary>
            ERR_IMMUTABLE_COPY_IS_MUTABLE = 256,

            RTT_TEST_PASSED = 512,
            EQUALITY_TEST_PASSED = 1024,
            ERR_ORIGINAL_EQUALITY_NONIDENTICAL = 2048,
            ERR_COPY_EQUALITY_NONIDENTICAL = 4096,
            ERR_COPY_REFERENCEEQUALS_ORIGINAL = 8192
        }

        /// <summary>
        /// Verifies that an externally-defined implementer of IMutableCacheDBLookup behaves as expected.<br />
        /// Expected behavior is that any instance returned by IMutableCacheDBLookup.ImmutableCopy() is equal
        /// to its original, equal to itself, that the copy is also equal to itself, and that equality between
        /// original and copy is commutative.
        /// </summary>
        /// <returns><c>true</c>, if the instance exhibits expected equality behavior.</returns>
        /// <param name="ext_Lookup">Ext lookup.</param>
        /// <param name="verificationResult">Verification result.</param>
        protected static bool Verify_External_IMutableLookup_ImmutableCopy_Equality
            (IMutableCacheDBLookup ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            verificationResult |= External_IMutableLookup_VerificationFlags.TESTED_EQUALITY;

            ICacheDBLookup copy = ext_Lookup.ImmutableCopy();
            bool identityError = false;
            if (!ext_Lookup.Equals(ext_Lookup)) // Compiler warning is OK
            {
                identityError = true;
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_ORIGINAL_EQUALITY_NONIDENTICAL;
            }

            if (!copy.Equals(copy)) // Compiler warning OK again
            {
                identityError = true;
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_COPY_EQUALITY_NONIDENTICAL;
            }

            if (ext_Lookup.Equals(copy))
            {
                if (copy.Equals(ext_Lookup))
                {
                    verificationResult |= External_IMutableLookup_VerificationFlags.EQUALITY_TEST_PASSED;
                    return !identityError;
                }

                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_EQUALITY_NONCOMMUTATIVE;
            }

            verificationResult |= External_IMutableLookup_VerificationFlags.ERR_ORIGINAL_NOT_EQUALTO_COPY;

            return false;
        }


        /// <summary>
        /// Verifies that an externally-defined implementer of IMutableCacheDBLookup behaves as expected.<br />
        /// Expected behavior is that IMutableCacheDBLookup.ImmutableCopy() returns
        /// an instance of ICacheDBLookup whose RTT excludes IMutableCacheDBLookup.<br />
        /// <br />
        /// In other words, when we ask a mutable to give us an *immutable* copy,
        /// then we cannot accept any "copy" which is still mutable.
        /// </summary>
        /// <remarks>
        /// This method only needs to be called when working with external implementations of
        /// IMutableCacheDBLookup. If an internal instance is passed instead, the result will
        /// be flagged with <see cref="External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED;"/> (and
        /// possibly also with <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE;"/>,
        /// if it was an unexpected internal type.
        /// </remarks>
        protected static bool Verify_External_IMutableLookup_ImmutableCopy_Valid
             (IMutableCacheDBLookup ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            ICacheDBLookup immutable = ext_Lookup.ImmutableCopy();
            bool result = !ReferenceEquals(immutable, ext_Lookup);

            if (!result) // Verification fails if copy is reference-equal to original
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_COPY_REFERENCEEQUALS_ORIGINAL;
            }

            if (immutable is IMutableCacheDBLookup) // Verification fails
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_IMMUTABLE_COPY_IS_MUTABLE;
                result = false;
            }

            if (immutable is ICacheDBLookup && result) // Verification succeeds
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED;
            }

            return result;
        }

        /// <summary>
        /// Examines the runtime type of an IMutableCacheDBLookup
        /// </summary>
        /// <returns><c>true</c>, always.</returns>
        /// <param name="ext_Lookup">Ext lookup.</param>
        /// <param name="verificationResult">Result of the verification check. If the object has external or
        /// expected internal type,
        /// the <see cref="External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED"/> flag will be set.</param>
        /// <remarks>
        /// This method only needs to be called when working with external implementations of
        /// IMutableCacheDBLookup. If an internal instance is passed instead, the result may possibly
        /// be flagged with <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE;"/>,
        /// if it was an unexpected internal type.
        /// </remarks>
        protected static bool Determine_IMutableLookup_RTT_is_External_or_Internal
            (IMutableCacheDBLookup ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            verificationResult |= External_IMutableLookup_VerificationFlags.TESTED_RTT;

            if (ext_Lookup is MutableCacheDBLookup) // Verification succeeds (RTT_TEST_PASSED flag set)
            {
                // (in this case, we didn't really need to perform verification in the first place)
                verificationResult |= External_IMutableLookup_VerificationFlags.INTERNAL_INSTANCE_MCDL;
            }

            // This should never happen, but if it does, it will succeed with trouble code
            // Compiler warning indicates that everything is okay! It's just warning
            // us that we're checking for something that *should* be impossible ;)
            else if (ext_Lookup is MutableDBLookup)
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.INTERNAL_INSTANCE_MDBL
                        | External_IMutableLookup_VerificationFlags.RTT_TROUBLE;
            }

            // This should never happen, but if it does, it will succeed with trouble code
            else if (ext_Lookup is DBLookupBase)
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.INTERNAL_INSTANCE_DBLB
                        | External_IMutableLookup_VerificationFlags.RTT_TROUBLE;
            }

            // Instances of externally-defined types will fall into this "else" case
            else
            {
                ; // Actually we dont need to do anything... for now
            }

            return true;
        }

        /// <summary>
        /// Gets the failure instance.
        /// </summary>
        /// <returns>The failure instance.</returns>
        /// <param name="query">Query.</param>
        /// <exception cref="ArgumentNullException"><c>query</c> is <c>null</c>.</exception>
        /// <exception cref="SecurityException">Runtime type of <c>query</c> is not an 
        /// appropriate internally-recognized type.</exception>
        public static ILookupResult<ICacheDBLookup> GetFailureInstance(ICacheDBLookup query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (query is MutableCacheDBLookup mutableCacheDBLookup)
                return GetFailureInstance_Internal(mutableCacheDBLookup.ImmutableCopy_Internal());

            if (query is CacheDBLookup lookup)
                return GetFailureInstance_Internal(lookup) as ILookupResult<ICacheDBLookup>;

            if (query is IMutableCacheDBLookup external_mutableCacheDBLookup)
            {
                External_IMutableLookup_VerificationFlags
                    verificationFlags = Verify_External_IMutableLookup_ImmutableCopy_NotReturn_InstanceOf_IMutableLookup(external_mutableCacheDBLookup);
                if (
                    .HasFlag(External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED))
                        return GetFailureInstance(external_mutableCacheDBLookup.ImmutableCopy());
            }

            else
                throw new SecurityException("Externally-defined types may not be passed as parameter to CacheDBLookupManager.GetFailureInstance(ICacheDBLookup)");
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
            bool cacheFirst = Policy.HasFlag(CacheDBPolicy.PREFER_CACHE);

            if (cacheFirst && clp.CacheContains(query))
                result = CacheLookup(query);
            else
            {
                bool gotFromDatabase = false;
                try
                {
                    DBLookupResult dbResult = DatabaseLookup(query);
                    result = CacheDBLookupResult.Build_Internal(dbResult, this, DataSource.DATABASE);
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

        public ILookupResult Lookup(ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            return Lookup(BuildLookup(query, bypassCache, dontCacheResult));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.Cache.CacheDBLookupManager"/> can custom cache lookup.
        /// </summary>
        /// <value><c>true</c> if can custom cache lookup; otherwise, <c>false</c>.</value>
        public bool CanCustomCacheLookup { get { return Policy.HasFlag(CacheDBPolicy.ALLOW_CUSTOM_CACHE_LOOKUP); } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.Cache.CacheDBLookupManager"/> can custom cache insert.
        /// </summary>
        /// <value><c>true</c> if can custom cache insert; otherwise, <c>false</c>.</value>
        public bool CanCustomCacheInsert { get { return Policy.HasFlag(CacheDBPolicy.ALLOW_CUSTOM_CACHE_INSERT); } }

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
