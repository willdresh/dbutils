// // MutableDBLookup_Equality.cs
// // Will Dresh
// // w@dresh.app
using System;
using VerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;
namespace DBInterface
{
    public sealed partial class MutableDBLookup
        :IMutableLookup<DBLookupBase>, IMutableLookup<ILookup>, IEquatable<MutableDBLookup>
    {

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


            if (other is MutableLookup int_ml)
                return Equals(int_ml.Unwrap_Immutable);
            if (other is IMutableLookup<ILookup> ext_ilu)
            {
                if (!LookupManager.VerifyInstance(ext_ilu, out VerificationFlags flags))
                    throw new CustomTypeFailedVerificationException(flags);

                return Equals(ext_ilu.ImmutableCopy());
            }

            if (other is DBLookupBase int_dblb)
                return Equals(int_dblb);

            return ImmutableCopy_Internal().Equals(other);
        }
    }
}
