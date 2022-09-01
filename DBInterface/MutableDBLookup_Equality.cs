// // MutableDBLookup_Equality.cs
// // Will Dresh
// // w@dresh.app
using System;
using VerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;
namespace DBInterface
{
    public sealed partial class MutableDBLookup
        :IMutableLookup<DBLookupBase>, IMutableLookup<ILookup>, IEquatable<MutableDBLookup>,
        ILookup
    {
        private static bool VerifyInstance(IMutableLookup<DBLookupBase> ext_mu_dblb, out VerificationFlags flags)
            => VerifyInstance(ext_mu_dblb as IMutableLookup<ILookup>, out flags);

        private static bool VerifyInstance(IMutableLookup<ILookup> ext_mu_ext_lu, out VerificationFlags flags)
            => LookupManager.VerifyInstance(ext_mu_ext_lu, out flags);

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
        private ILookup UnwrapMutables(ILookup other)
        {
            if (other is MutableDBLookup int_mdbl)
                return int_mdbl.Unwrap_Immutable;
            if (other is MutableLookup int_ml)
                return int_ml.Unwrap_Immutable;
            if (other is IMutableLookup<DBLookupBase> ext_mu_dblb)
            {
                if (!VerifyInstance(ext_mu_dblb, out VerificationFlags flags))
                    throw new CustomTypeFailedVerificationException("ext_mu_dblb", ext_mu_dblb, flags);
                return ext_mu_dblb.ImmutableCopy();
            }
            if (other is IMutableLookup<ILookup> ext_mu_ilu)
            {
                if (!VerifyInstance(ext_mu_ilu, out VerificationFlags flags))
                    throw new CustomTypeFailedVerificationException("ext_mu_ilu", ext_mu_ilu, flags);
                return ext_mu_ilu.ImmutableCopy();
            }

            return other;
        }

        public bool Equals(DBLookupBase dblb)
        {
            return dblb.Equals(this.Unwrap_Immutable);
        }

        /// <summary>
        /// Aliases <see cref="DBLookupBase.Equals(DBLookupBase)"/>
        /// </summary>
        public bool Equals(MutableDBLookup other)
        {
            return other.dbl.Equals(this.dbl);
        }

        /// <summary>
        /// Implements RTT checks to perform comparison of (possibly externally-defined) types. <br />
        /// This may trigger a verification check by <see cref="LookupManager.VerifyInstance(IMutableLookup{ILookup}, out VerificationFlags)"/>
        /// when using externally-defined types.
        /// </summary>
        /// <exception cref="MutableDBLookup.CustomTypeFailedVerificationException">Verification Failed</exception>
        public bool Equals(ILookup other)
        {
            if (ReferenceEquals(this, other))
                return true;

            var unwrapped = UnwrapMutables(other);

            if (unwrapped is MutableDBLookup int_mdbl)
                return Equals(int_mdbl);

            if (unwrapped is DBLookupBase int_dblb)
                return Equals(int_dblb);

            return ImmutableCopy_Internal().Equals(other);
        }
    }
}
