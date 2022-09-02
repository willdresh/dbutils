// // LookupManager.cs
// // Will Dresh
// // w@dresh.app

using System;
namespace DBInterface
{
    /// <summary>
    /// Lookup Manager. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    public abstract partial class LookupManager: ILookupProvider<ILookup>
    {

        [Flags]
        public enum LookupPolicy { ALLOW_LOOKUP = 1 }
        public const LookupPolicy DEFAULT_Policy = LookupPolicy.ALLOW_LOOKUP;

        public LookupPolicy Policy { get; }

        // Constructor must remain internal to prevent external inheritance
        internal LookupManager(LookupPolicy policy = DEFAULT_Policy)
        {
            Policy = policy;
        }

        /// <summary>
        /// Does the lookup.
        /// </summary>
        /// <returns>The result of the lookup operation.</returns>
        /// <param name="query">Query (Precondition: the runtime type of <c>query</c>
        ///  is immutable - i.e. implements ILookup AND DOES NOT implement IMutableLookup)</param>
        protected abstract ILookupResult<ILookup> DoLookup(ILookup query);

        public bool LookupAllowed { get => Policy.HasFlag(LookupPolicy.ALLOW_LOOKUP); }

        /// <exception cref="CustomTypeFailedVerificationException"><c>query</c> is of externally-defined type, and did not pass internal verification.</exception>
        private ILookupResult<ILookup> DoLookup_Private(ILookup query)
        {
            ILookup immutableQuery = query;
            if (query is IMutableLookup<ILookup>mutable) // Behavior of likely-external origin
            {
                ILookup immutable = null;
                External_IMutableLookup_VerificationFlags flags = 0;
                if (mutable is CacheDB.MutableCacheDBLookup int_mutablecdb)
                    immutable = int_mutablecdb.Unwrap_Immutable;
                else if (mutable is MutableDBLookup int_mutabledb)
                    immutable = int_mutabledb.Unwrap_Immutable;
                else if (mutable is MutableLookup int_mutable)
                    immutable = int_mutable.Unwrap_Immutable;
                else if (VerifyInstance(mutable, out flags))
                    immutable = mutable.ImmutableCopy();
                else
                    throw new CustomTypeFailedVerificationException(flags);

                immutableQuery = immutable;
            }

            return DoLookup(immutableQuery);
        }

        /// <summary>
        /// Lookup the specified query by calling <see cref="LookupManager.DoLookup(ILookup)"/>
        /// </summary>
        /// <returns>The result of the lookup operation.</returns>
        /// <param name="query">Query.</param>
        /// <exception cref="ArgumentNullException"><c>query</c> is <c>null</c>.</exception>
        /// <exception cref="LookupNotPermittedException">Lookup is not allowed by policy</exception>
        /// <exception cref="CustomTypeFailedVerificationException"><c>query</c> is of externally-defined type, and did not pass internal verification.</exception>
        /// <remarks>
        /// In case <c>query</c> implements IMutableLookup, then verification will
        /// be triggered; if successful an immutable instance will be created via <c>IMutableLookup.ImmutableCopy()</c>,
        /// and it is this immutable copy - NOT the original - that will be processed by the base class override of
        /// <see cref="LookupManager.DoLookup(ILookup)"/>
        /// </remarks>
        public ILookupResult<ILookup> Lookup(ILookup query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (LookupAllowed)
                DoLookup_Private(query);

            throw new LookupNotPermittedException();
        }

        /// <summary>
        /// When an external implementation of <see cref="IMutableLookup"/>
        /// is passed to certain methods of derived classes of <see cref="LookupManager"/>,
        ///  then a (protected-access) verification check is performed.<br />
        /// The result of this verification operation is an instance
        /// of <c>External_IMutableLookup_VerificationFlags</c>.
        /// </summary>
        [Flags]
        public enum External_IMutableLookup_VerificationFlags
        {
            /// <summary>
            /// This flag indicates that the RTT of an IMutableLookup
            /// is an internally-defined type. Accompanies the
            /// <see cref="External_IMutableLookup_VerificationFlags;"/> flag
            /// as the automatic result of submitting an expected internal instance for verification.
            /// </summary>
            /// <remarks>
            /// Only one or zero <c>INTERNAL_INSTANCE_*</c> flags should be set per verification;
            /// if more than one is set this may indicate an internal bug.
            /// </remarks>
            INTERNAL_INSTANCE_MCDL = 1,

            /// <summary>
            /// This flag indicates that the RTT of an IMutableLookup
            /// is an internally-defined type. This flag SHOULD never get set,
            /// however this flag has been added for possible future use.<br />
            /// Presently, when this flag is (somehow, impossibly) set, the 
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
            /// This flag indicates that the RTT of an IMutableLookup
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
            /// This flag indicates whether <c>IMutableLookup.ImmutableCopy()</c>
            /// has been called on an instance in order to test its integrity
            /// for use with <see cref=" LookupManager"/>. This should be
            /// true for externally-defined types, and false for internally-defined ones.
            /// </summary>
            TESTED_RTT = 8,

            /// <summary>
            /// This flag indicates whether an <c>IMutableLookup</c> instance
            /// succeeded verification for use with <see cref="LookupManager"/>.
            /// </summary>
            TESTED_EQUALITY = 16,

            /// <summary>
            /// This flag indicates that something unexpected occurred while trying
            /// to infer the RTT of an <c>IMutableLookup</c> instance. When
            /// this flag is set, it may indicate a bug, or that <see cref="LookupManager"/>
            /// is being used in a manner other than it was designed.
            /// </summary>
            RTT_TROUBLE = 32,

            /// <summary>
            /// This flag indicates that the equality methods for an instance of
            /// externally-defined <c>IMutableLookup</c> instance have been
            /// found to exhibit noncommutative behavior, resulting in verification failure.
            /// </summary>
            ERR_EQUALITY_NONCOMMUTATIVE = 64,

            /// <summary>
            /// This flag indicates that the implementation of <c>IMutableLookup.ImmutableCopy()</c>
            /// yielded a copy which was "not equal" to the original,
            /// resulting in verification failure.
            /// </summary>
            ERR_ORIGINAL_NOT_EQUALTO_COPY = 128,

            /// <summary>
            /// This flag indicates that the implementation of <c>IMutableLookup.ImmutableCopy()</c>
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
        /// Verifies that an externally-defined implementer of IMutableLookup<ILookup>behaves as expected.<br />
        /// Expected behavior is that any instance returned by IMutableLookup.ImmutableCopy() is equal
        /// to its original, equal to itself, that the copy is also equal to itself, and that equality between
        /// original and copy is commutative.
        /// </summary>
        /// <returns><c>true</c>, if the instance exhibits expected equality behavior.</returns>
        /// <param name="ext_Lookup">Ext lookup.</param>
        /// <param name="verificationResult">Verification result.</param>
        /// <exception cref="ArgumentNullException"><c>ext_Lookup</c> is null</exception>
        private static bool Verify_External_IMutableLookup_ImmutableCopy_Equality
            (IMutableLookup<ILookup> ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            if (ext_Lookup == null) throw new ArgumentNullException(nameof(ext_Lookup));

            verificationResult |= External_IMutableLookup_VerificationFlags.TESTED_EQUALITY;



            ILookup copy = ext_Lookup.ImmutableCopy();
            bool identityError = false;
            if (ReferenceEquals(ext_Lookup, copy))
            {
                identityError = true;
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_COPY_REFERENCEEQUALS_ORIGINAL;
            }

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

            bool originalEqualsCopy = ext_Lookup.Equals(copy);
            bool copyEqualsOriginal = copy.Equals(ext_Lookup);

            if (originalEqualsCopy || copyEqualsOriginal)
            {
                if (originalEqualsCopy ^ copyEqualsOriginal)
                    verificationResult |= External_IMutableLookup_VerificationFlags.ERR_EQUALITY_NONCOMMUTATIVE;
                else
                {
                    // At this point this method's equality verification succeeds
                    verificationResult |= External_IMutableLookup_VerificationFlags.EQUALITY_TEST_PASSED;
                    return !identityError; // Identity error, in this case, should only happen due to reference-equality
                }
            }

            verificationResult |= External_IMutableLookup_VerificationFlags.ERR_ORIGINAL_NOT_EQUALTO_COPY;
            return false;
        }


        /// <summary>
        /// Verifies that an externally-defined implementer of IMutableLookup<ILookup>behaves as expected.<br />
        /// Expected behavior is that IMutableLookup.ImmutableCopy() returns
        /// an instance of ILookup whose RTT excludes IMutableLookup.<br />
        /// <br />
        /// In other words, when we ask a mutable to give us an *immutable* copy,
        /// then we cannot accept any "copy" which is still mutable.
        /// </summary>
        /// <remarks>
        /// This method only needs to be called when working with external implementations of
        /// IMutableLookup. If an internal instance is passed instead, the result will
        /// possibly be flagged with <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE;"/>,
        /// if it was an unexpected internal type.
        /// </remarks>
        private static bool Verify_External_IMutableLookup_ImmutableCopy_RTT
             (IMutableLookup<ILookup> ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            verificationResult |= External_IMutableLookup_VerificationFlags.TESTED_RTT;

            ILookup immutable = ext_Lookup.ImmutableCopy();
            bool result = !ReferenceEquals(immutable, ext_Lookup);

            if (!result) // Verification fails if copy is reference-equal to original
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_COPY_REFERENCEEQUALS_ORIGINAL;
            }

            if (immutable is IMutableLookup<ILookup>) // Verification fails
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.ERR_IMMUTABLE_COPY_IS_MUTABLE;
                result = false;
            }

            if (immutable is ILookup && result) // Verification succeeds
            {
                verificationResult |= External_IMutableLookup_VerificationFlags.RTT_TEST_PASSED;
            }

            return result;
        }

        /// <summary>
        /// Examines the runtime type of an IMutableLookup<br />
        /// May set the various <c>External_IMutableLookup_VerificationFlags.INTERNAL_INSTANCE_*</c> flags,
        /// or the <c>External_IMutableLookup_VerificationFlags.RTT_TROUBLE</c> flag.
        /// </summary>
        /// <returns><c>true</c>, if we can safely bypass verification checks because the supplied instance
        /// was of an expected internal type. <c>false</c> otherwise.</returns>
        /// <param name="ext_Lookup">A lookup which may be externally-defined</param>
        /// <param name="verificationResult">Result flags of the verification check</param>
        /// <remarks>
        /// This method only needs to be called when working with external implementations of
        /// IMutableLookup. If an internal instance is passed instead, the result may possibly
        /// be flagged with <see cref="External_IMutableLookup_VerificationFlags.RTT_TROUBLE;"/>,
        /// if it was an unexpected internal type.
        /// </remarks>
        private static bool Determine_IMutableLookup_RTT_is_Internal
            (IMutableLookup<ILookup> ext_Lookup, ref External_IMutableLookup_VerificationFlags verificationResult)
        {
            if (ext_Lookup is MutableLookup) // Verification succeeds (RTT_TEST_PASSED flag set)
            {
                // (in this case, we didn't really need to perform verification in the first place)
                verificationResult |= External_IMutableLookup_VerificationFlags.INTERNAL_INSTANCE_MCDL;
                return true;
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
            //else
            //{
            //    return false; // Actually we dont need to do anything... for now
            //}

            return false;
        }

        protected internal static bool VerifyInstance(IMutableLookup<ILookup> ext_Lookup, out External_IMutableLookup_VerificationFlags flags)
        {
            flags = 0;
            if (Determine_IMutableLookup_RTT_is_Internal(ext_Lookup, ref flags))
                return true;

            return Verify_External_IMutableLookup_ImmutableCopy_RTT(ext_Lookup, ref flags)
                && Verify_External_IMutableLookup_ImmutableCopy_Equality(ext_Lookup, ref flags);
        }

        protected internal static bool VerifyInstance(IMutableLookup<ILookup> ext_Lookup)
        {
            return VerifyInstance(ext_Lookup, out External_IMutableLookup_VerificationFlags flags);
        }
    }
}
