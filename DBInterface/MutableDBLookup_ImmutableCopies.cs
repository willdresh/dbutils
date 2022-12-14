// // MutableDBLookup_ImmutableCopies.cs
// // Will Dresh
// // w@dresh.app
using System;
namespace DBInterface
{
    public sealed partial class MutableDBLookup
        :IMutableLookup<DBLookupBase>, IMutableLookup<ILookup>, IEquatable<MutableDBLookup>,
        ILookup
    {

        internal DBLookup ImmutableCopy_Internal()
        {
            return new DBLookup(this);
        }

        DBLookupBase IMutableLookup<DBLookupBase>.ImmutableCopy()
        {
            return ImmutableCopy_Internal();
        }

        public ILookup ImmutableCopy()
        {
            return ImmutableCopy_Internal();
        }
    }
}
