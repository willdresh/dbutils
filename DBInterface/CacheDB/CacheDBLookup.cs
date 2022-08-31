// // CacheLookup.cs
// // Will Dresh
// // w@dresh.app

using System;
using System.Data;

namespace DBInterface.CacheDB
{
    public enum DataSource { CACHE, DATABASE, NONE }

    public interface ICacheDBLookup: ILookup
    {
        bool BypassCache { get; }
        bool DontCacheResult { get; }
    }

    public interface ICacheDBLookupResult: ILookupResult<ICacheDBLookup>, ILookupResult<ILookup>
    {
        DataSource ActualSource { get; }
    }

    /// <summary>
    /// CacheDB Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal class CacheDBLookup: DBLookup, ICacheDBLookup, IEquatable<CacheDBLookup>
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
        public static ICacheDBLookup Build(DBLookupManager mgr, ILookup query, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (mgr == null)
                throw new ArgumentNullException(nameof(mgr));

            return Build_Internal(mgr, query, bypassCache, dontCacheResult);
        }

        /// <summary>
        /// Build a lookup 
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>dbLookup</c> is <c>null</c>,</exception>
        public static ICacheDBLookup Build(DBLookupBase dbLookup, bool bypassCache = false, bool dontCacheResult = false)
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
    /// Mutable wrapper for <see cref="Type:CacheDBLookup"/>. This class cannot be
    /// externally inherited, as it has no public constructors.
    /// </summary>
    public sealed class MutableCacheDBLookup
        :IMutableLookup<DBLookupBase>, IMutableLookup<ICacheDBLookup>, IMutableLookup<ILookup>
    {
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

        private MutableCacheDBLookup(string key = null, IDbConnection dbConnection = null, bool bypassCache = false, bool dontCacheResult = false)
        {
            cdlu = new CacheDBLookup(key, dbConnection, bypassCache, dontCacheResult);
        }

        internal IDbConnection DBConnection { get => cdlu.DBConnection; set => cdlu.DBConnection = value; }
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

        public static IMutableLookup<ICacheDBLookup> Build(CacheDBLookupManager manager, ILookup lu = null, bool bypassCache = false, bool dontCacheResult = false)
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
        public static IMutableLookup<ICacheDBLookup> Build_Mutable_Copy(ICacheDBLookup lookup)
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
        /// <see cref="ICacheDBLookup.BypassCache"/>
        /// and <see cref="ICacheDBLookup.DontCacheResult"/>)
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

        ICacheDBLookup IMutableLookup<ICacheDBLookup>.ImmutableCopy()
        {
            return new CacheDBLookup(this);
        }

        public bool Equals(ILookup other)
        {
            string otherKey = (other is Lookup int_other ?
                int_other.Key_Internal :
                other.KeyCopy);

            bool result = cdlu.Key_Internal == null ? otherKey == null : cdlu.Key_Internal.Equals(otherKey);

            if (other is ICacheDBLookup cdbl)
            {
                result = result && Equals(cdbl);
            }
            if (other is MutableDBLookup int_mdbl)
            {
                result = result && ReferenceEquals(cdlu.DBConnection, int_mdbl.Unwrap_Immutable.DBConnection);
            }
            else if (other is IMutableLookup<DBLookupBase> ext_mdbl)
            {
                result = result && ReferenceEquals(cdlu.DBConnection, ext_mdbl.ImmutableCopy().DBConnection);
            }

            if (other is DBLookupBase dblb)
            {
                result = result && ReferenceEquals(cdlu.DBConnection, dblb.DBConnection);
            }

            return result;
        }

        public bool Equals(DBLookupBase other)
        {
            if (other == null) return false;

            bool result = other.Equals(cdlu); // DBLookupBase.Equals(DBLookupBase)

            if (other is IMutableLookup<ILookup> ext_iml)
                return Equals(ext_iml.ImmutableCopy());

            if (other is ICacheDBLookup cdbl)
            {
                result &= cdbl.BypassCache == BypassCache;
                result &= cdbl.DontCacheResult == DontCacheResult;
            }

            return result;
        }

        public bool Equals(ICacheDBLookup other)
        {
            if (other == null) return false;

                // Unwrap mutables
                if (other is MutableDBLookup mutable_dblb)
                return Equals(mutable_dblb.Unwrap_Immutable);
            if (other is IMutableLookup<DBLookupBase> ext_mutable_dblb)
                return Equals(ext_mutable_dblb.ImmutableCopy());

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
                result = result && dblb.Equals(this);

            return result;
        }
    }


    /// <summary>
    /// CacheDB Lookup result. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    internal class CacheDBLookupResult: ILookupResult<ILookup>, ILookupResult<ICacheDBLookup>
    {

        internal CacheDBLookupResult(ICacheDBLookup query, object response, DataSource src)
        { ActualSource = src; Query = query;  Response = response; }

        public DataSource ActualSource { get; }
        public object Response { get; }
        public ICacheDBLookup Query { get; }
        ILookup ILookupResult<ILookup>.Query { get { return Query; } }

        internal static CacheDBLookupResult Build_Internal(ICacheDBLookup query, object response, DataSource src)
        {
            return new CacheDBLookupResult(query, response, src);
        }

        public static ILookupResult<ICacheDBLookup> Build(ICacheDBLookup query, object response, DataSource src)
        {
            return Build_Internal(query, response, src);
        }
    }

    /// <summary>
    /// Simple factory class to provide <see cref="ICacheDBLookup"/> instances for
    /// use with <see cref="T:CacheDBLookupManager"/> 
    /// </summary>
    public static class CacheDBLookupFactory
    {
        public static ICacheDBLookup BuildLookup(CacheDBLookupManager manager, ILookup lookup, bool bypassCache = false, bool dontCacheResult = false)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (lookup == null) throw new ArgumentNullException(nameof(lookup));

            if (lookup is DBLookup dbl) // Using a more-derived type, when possible, will prevent extraneous copy operations
                return new CacheDBLookup(dbl, bypassCache, dontCacheResult);
            if (lookup is DBLookupBase dblb)
                return new CacheDBLookup(dblb, bypassCache, dontCacheResult);

            throw new NotSupportedException("No support for the runtime type of the 'lookup' parameter");
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
