// // MutableCacheDBLookup.cs
// // Will Dresh
// // w@dresh.app
using System;
using VerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;

namespace DBInterface.CacheDB
{
    /// <summary>
    /// Mutable wrapper for <see cref="Type:CacheDBLookup"/>. This class cannot be
    /// externally inherited, as it has no public constructors.
    /// </summary>
    public sealed class MutableCacheDBLookup: 
        IMutableLookup<DBLookupBase>,
        IMutableLookup<ICacheLookup>, IMutableLookup<CacheDBLookup>, ICacheLookup,
        IMutableLookup<ILookup>, ILookup
    {
        public sealed class MCDBLCustomTypeFailedVerificationException : SecurityException
        {
            private static readonly string CTFVEMessage = "A custom mutable instance failed internal integrity checks in MutableCacheDBLookup";

            public VerificationFlags Flags { get; }
            public object Instance { get; }

            internal MCDBLCustomTypeFailedVerificationException(string typeDescription, object instance, VerificationFlags flags)
                : base(GenerateMessage(typeDescription, instance))
            {
                Flags = flags;
                Instance = instance;
            }

            internal MCDBLCustomTypeFailedVerificationException(VerificationFlags flags)
                : base(CTFVEMessage)
            { Flags = flags; }

            private static string GenerateMessage(string typeDescription, object instance)
            {
                if (typeDescription == null || instance == null) return String.Format("(unexpected: null parameter) {0}", CTFVEMessage);
                return String.Format("{0} --- Custom description of type is {1} --- instance.ToString(): {2}", CTFVEMessage, typeDescription, instance.ToString());
            }
        }

        public sealed class InternalInstanceExpectedException: SecurityException
        {
            private static readonly string IIEEMessage = "Internal MutableCacheDBLookup instance expected";
            internal InternalInstanceExpectedException()
                : base(IIEEMessage) { }

            internal InternalInstanceExpectedException(string message)
                : base(GenerateMessage(message)) { }

            internal InternalInstanceExpectedException(string message, Exception innerException)
                : base(GenerateMessage(message), innerException) { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", IIEEMessage, msg);
            }
        }

        private CacheDBLookup cdlu;

        private MutableCacheDBLookup(string key = null, System.Data.IDbConnection dbConnection = null, bool bypassCache = false, bool dontCacheResult = false)
        {
            cdlu = new CacheDBLookup(key, dbConnection, bypassCache, dontCacheResult);
        }

        internal System.Data.IDbConnection DBConnection { get => cdlu.DBConnection; set => cdlu.DBConnection = value; }
        internal string Key_Internal { get => cdlu.Key_Internal; set => cdlu.Key_Internal = value; }
        public string KeyCopy { get => cdlu.KeyCopy; set => cdlu.KeyCopy = value; }
        public bool BypassCache { get => cdlu.BypassCache; set => cdlu.BypassCache = value; }
        public bool DontCacheResult { get => cdlu.BypassCache; set => cdlu.DontCacheResult = value; }

        #region Static Builder Methods
        /// <summary>
        /// Builds an instance (internal use only).
        /// </summary>
        /// <returns>A new instance.</returns>
        /// <param name="query">Query. Null is not allowed.</param>
        /// <exception cref="ArgumentNullException"><c>query</c> is <c>null</c></exception>
        internal static MutableCacheDBLookup Build_Internal(DBLookupBase query, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return new MutableCacheDBLookup(query.Key_Internal, query.DBConnection, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Builds an instance (internal use only).
        /// </summary>
        /// <returns>A new instance.</returns>
        /// <param name="manager">Manager instance on which lookup is to be performed. Null is not allowed.</param>
        /// <param name="lookup">Provides the key needed for lookup operations. Null is allowable.</param>
        /// <exception cref="ArgumentNullException"><c>manager</c> is <c>null</c></exception>
        internal static MutableCacheDBLookup Build_Internal(CacheDBLookupManager manager, ILookup lookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            string lookupKey =
                (lookup is Lookup int_lookup ?
                    int_lookup.Key_Internal : 
                    (lookup == null ?
                        null :
                    lookup.KeyCopy)
                );
            return new MutableCacheDBLookup(lookupKey, manager.connection, bypassCache, dontCacheResult);
        }

        public static IMutableLookup<ICacheLookup> Build(CacheDBLookupManager manager, ILookup lu = null, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            return Build_Internal(manager, lu, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Builds a mutable copy of the supplied <c>CacheDBLookup</c> instance
        /// (internal use only).
        /// </summary>
        /// <param name="lookup">The instance to be copied. <c>null</c> is not allowed.</param>
        /// <returns>A mutable copy of <c>lookup</c></returns>
        /// <exception cref="ArgumentNullException"><c>lookup</c> is <c>null</c></exception>
        internal static MutableCacheDBLookup Build_Mutable_Copy_Internal(CacheDBLookup lookup)
        {
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));

            return new MutableCacheDBLookup(lookup.Key_Internal, lookup.DBConnection, lookup.BypassCache, lookup.DontCacheResult);
        }

        /// <summary>
        /// Builds and returns a mutable copy.
        /// </summary>
        /// <returns>The mutable copy.</returns>
        /// <param name="lookup">Lookup. Runtime type is required to be an instance of <see cref="DBLookupBase"/>,
        /// for security and integrity purposes.</param>
        /// <exception cref="Type:SecurityException">thrown in case the runtime type of <c>lookup</c>
        /// is not an instance of <see cref="Type:DBLookupBase"/>. Generally this indicates that
        /// the caller tried to pass an instance of externally-defined type, which might violate security</exception>
        public static IMutableLookup<ICacheLookup> Build_Mutable_Copy(ICacheLookup lookup)
        {
            if (lookup is DBLookupBase dblb)
                return new MutableCacheDBLookup(dblb.Key_Internal, dblb.DBConnection, lookup.BypassCache, lookup.DontCacheResult);

            throw new InternalInstanceExpectedException();
        }

        internal static MutableCacheDBLookup Build_Mutable_Copy_Internal(DBLookupBase lookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            return new MutableCacheDBLookup(lookup.Key_Internal, lookup.DBConnection, bypassCache, dontCacheResult);
        }

        #endregion

        #region Immutable Copying / Unwrapping

        /// <summary>
        /// Obtain a direct reference to the <see cref="CacheDBLookup"/> wrapped by this
        /// mutable accessor. CAUTION - for security/integrity, this reference should never
        /// be exposed externally.
        /// </summary>
        /// <returns>the <see cref="CacheDBLookup"/> wrapped by this instance</returns>
        internal CacheDBLookup Unwrap_Immutable
        {
            get { return cdlu; }
        }

        internal CacheDBLookup ImmutableCopy_Internal()
        {
            return new CacheDBLookup(this);
        }

        /// <summary>
        /// Obtain an immutable copy of <c>this</c>.
        /// </summary>
        /// <returns>The copy.</returns>
        public DBLookupBase ImmutableCopy()
        {
            return ImmutableCopy_Internal();
        }

        /// <summary>
        /// Obtain a newly-constructed <see cref="DBLookupBase"/> by stripping the
        /// <c>CacheDB</c>-exclusive properties from this instance.
        /// </summary>
        /// <returns>
        /// An immutable instance of DBLookupBase that is a copy of
        /// <c>this</c>, but without the <c>CacheDB</c>-exclusive properties (i.e.
        /// <see cref="ICacheLookup.BypassCache"/>
        /// and <see cref="ICacheLookup.DontCacheResult"/>)
        /// </returns>
        DBLookupBase IMutableLookup<DBLookupBase>.ImmutableCopy()
        {
            return new CacheDBLookup(cdlu.Key_Internal, cdlu.DBConnection, cdlu.BypassCache, cdlu.DontCacheResult);
        }

        /// <summary>
        /// Obtain an immutable copy of <c>this</c> mutable instance.
        /// </summary>
        /// <returns>A newly-constructed immutable copy.</returns>
        ILookup IMutableLookup<ILookup>.ImmutableCopy()
        {
            return new CacheDBLookup(this);
        }

        ICacheLookup IMutableLookup<ICacheLookup>.ImmutableCopy()
        {
            return new CacheDBLookup(this);
        }

        #endregion

        private static bool VerifyInstance(IMutableLookup<ICacheLookup> ext_mu_icl, out VerificationFlags flags)
            => VerifyInstance(ext_mu_icl as IMutableLookup<ILookup>, out flags);

        private static bool VerifyInstance(IMutableLookup<CacheDBLookup> ext_mu_cdl, out VerificationFlags flags)
            => VerifyInstance(ext_mu_cdl as IMutableLookup<ILookup>, out flags);

        private static bool VerifyInstance(IMutableLookup<DBLookupBase> ext_mu_dblb, out VerificationFlags flags)
            => VerifyInstance(ext_mu_dblb as IMutableLookup<ILookup>, out flags);

        private static bool VerifyInstance(IMutableLookup<ILookup> ext_mu_ext_ilu, out VerificationFlags flags)
            => LookupManager.VerifyInstance(ext_mu_ext_ilu, out flags);

        /// <summary>
        /// Unwraps the mutables. Triggers verification for externally-defined types.
        /// </summary>
        /// <returns>
        /// An immutable copy, if the instance passed in was a mutable.
        /// If the instance passed in was NOT detected as a known mutable type,
        /// then returns <c>other</c>.
        /// </returns>
        /// <param name="other">Other.</param>
        /// <exception cref="CustomTypeFailedVerificationException"></exception>
        private static ILookup UnwrapMutables(ILookup other)
        {
            if (other is MutableCacheDBLookup int_mcdbl)
                return int_mcdbl.Unwrap_Immutable;
            if (other is MutableDBLookup int_mdbl)
                return int_mdbl.Unwrap_Immutable;
            if (other is MutableLookup int_ml)
                return int_ml.Unwrap_Immutable;
            if (other is IMutableLookup<CacheDBLookup> ext_mu_cdbl)
            {
                if (!VerifyInstance(ext_mu_cdbl, out VerificationFlags flags))
                    throw new MCDBLCustomTypeFailedVerificationException("ext_mu_cdbl", ext_mu_cdbl, flags);
                return ext_mu_cdbl.ImmutableCopy();
            }
            if (other is IMutableLookup<DBLookupBase> ext_mu_dblb)
            {
                if (!VerifyInstance(ext_mu_dblb, out VerificationFlags flags))
                    throw new MCDBLCustomTypeFailedVerificationException("ext_mu_dblb", ext_mu_dblb, flags);
                return ext_mu_dblb.ImmutableCopy();
            }
            if (other is IMutableLookup<ICacheLookup> ext_mu_icl)
            {
                if (!VerifyInstance(ext_mu_icl, out VerificationFlags flags))
                    throw new MCDBLCustomTypeFailedVerificationException("ext_mu_icl", ext_mu_icl, flags);
                return ext_mu_icl.ImmutableCopy();
            }
            if (other is IMutableLookup<ILookup> ext_mu_ilu)
            {
                if (!VerifyInstance(ext_mu_ilu, out VerificationFlags flags))
                    throw new MCDBLCustomTypeFailedVerificationException("ext_mu_ilu", ext_mu_ilu, flags);
                return ext_mu_ilu.ImmutableCopy();
            }

            return other;
        }

        public bool Equals(ILookup other)
        {
            if (ReferenceEquals(this, other)) return true;

            // If we are using an internal type derived from Lookup,
            //  then use our internal-only accessors to speed up key comparison
            string otherKey = (other is Lookup int_other ?
                int_other.Key_Internal :
                other.KeyCopy);

            if (cdlu.Key_Internal == null) {
                return otherKey == null;
            }
            if (!cdlu.Key_Internal.Equals(otherKey))
                return false;

            ILookup immutable = UnwrapMutables(other);

            if (immutable is CacheDBLookup cdbl)
                return cdbl.Equals(this);
            if (immutable is DBLookupBase dblb)
                return dblb.Equals(this);
            if (immutable is ICacheLookup icdbl)
                return Equals(icdbl);

            // If this return statement is reached, then only the
            // keys have been compared; this is probably a bad thing.
            return true;
        }

        internal bool Equals(CacheDBLookup other)
        {
            return ReferenceEquals(this, other) // Short-circuit when reference-equal
                || Equals(other as ICacheLookup) && Equals(other as DBLookup);
        }

        public bool Equals(DBLookupBase other)
        {
            if (other == null) return false;

            return other.Equals(cdlu);
        }

        public bool Equals(ICacheLookup other)
        {
            if (other == null) return false;

            string otherKey;

            // Avoid an extraneous copy operation when working with internally-defined types
            if (other is CacheDBLookup cdbl)
                otherKey = cdbl.Key_Internal;
            else
                otherKey = other.KeyCopy;

            bool result = other.BypassCache == BypassCache &&
                other.DontCacheResult == DontCacheResult &&
                otherKey.Equals(Key_Internal);

            if (other is DBLookupBase dblb)
                result &= dblb.Equals(this);

            return result;
        }

        CacheDBLookup IMutableLookup<CacheDBLookup>.ImmutableCopy()
        {
            return new CacheDBLookup(this);
        }

        bool IEquatable<CacheDBLookup>.Equals(CacheDBLookup other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Custom type failed verification exception. This class cannot be inherited.
        /// This class cannot be publicly constructed.
        /// </summary>
        public sealed class CustomTypeFailedVerificationException : SecurityException
        {
            private static readonly string CTFVEMessage = "A custom mutable instance failed internal integrity checks";

            public VerificationFlags VerificationFlags { get; }

            internal CustomTypeFailedVerificationException(VerificationFlags flags)
                : base(CTFVEMessage)
            { VerificationFlags = flags; }
        }
    }
}
