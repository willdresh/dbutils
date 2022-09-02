using DBInterface;
using System;

namespace DBInterface_XUnit_Tests.ExternalTypes
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal class ExternalLookup: ILookup, IEquatable<ILookup>
    {
        Lookup lu;

        public ExternalLookup() { lu = Lookup.Build("Example"); ExternalData = 2.71; }
        public ExternalLookup(string? key) { lu = Lookup.Build(key); ExternalData = 3.14; }

        public double ExternalData { get; set; }

        public virtual string KeyCopy => lu.KeyCopy;

        public virtual bool Equals(ILookup? other)
        {
            if (other == null) return false;

            string otherKeyCopy = other.KeyCopy;
            if (otherKeyCopy == null)
                return KeyCopy == null;

            if (other is IMutableLookup<ILookup> mutable)
                return Equals(mutable.ImmutableCopy());

            return lu.KeyCopy.Equals(otherKeyCopy);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is IMutableLookup<ILookup> mutable)
                return Equals(mutable.ImmutableCopy());

            if (obj is ILookup ilu)
                return Equals(ilu);

            return false;
        }
    }

    internal class BadExternalLookup: ExternalLookup, ILookup, IEquatable<ILookup>
    {
        public BadExternalLookup() : base() { }
        public BadExternalLookup(string? key) : base(key) { }

        public static readonly string AnyInstanceReturnsThisForKeyCopy = "Muahaha! I did not copy faithfully!";
        public override string KeyCopy => AnyInstanceReturnsThisForKeyCopy;

        public override bool Equals(ILookup? other)
        {
            if (other == null) return true;
            if (other.KeyCopy == null) return base.KeyCopy != null; // Workaround, since BadExternalLookup.KeyCopy is intended to work improperly
            return !base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return true;
            return !base.Equals(obj);
        }
    }
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
}
