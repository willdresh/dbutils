using System;
using DBInterface;

namespace DBInterface_XUnit_Tests.ExternalTypes
{
    public class BadExternalMutableLookup: IMutableLookup<ILookup>
    {
        private MutableLookup ml = MutableLookup.Build("Bad External Mutable Lookup (gives mutables instead of immutable copies)");

        public bool Equals(ILookup? other)
        {
            return ml.Equals(other);
        }

        public bool Equals(Lookup? other)
        {
            if (other == null) return false;
            return other.KeyEquals(ml.GetReadOnlyKey());
        }

        /// <summary>
        /// Bad method! Bad!
        /// </summary>
        /// <returns>A MutableLookup instance (wrong!)</returns>
        public ILookup ImmutableCopy()
        {
            return MutableLookup.Copy(ml);
        }
    }
}
