using DBInterface;
using System;

namespace DBInterface_XUnit_Tests.ExternalTypes
{
    internal class ExternalLookup: ILookup, IEquatable<ILookup>
    {
        Lookup lu;

        public ExternalLookup() { lu = Lookup.Build("Example"); ExternalData = 2.71; }
        public ExternalLookup(string key) { lu = Lookup.Build(key); ExternalData = 3.14; }

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
        public BadExternalLookup(string key) : base(key) { }

        public static readonly string AnyInstanceReturnsThisForKeyCopy = "Muahaha! I did not copy faithfully!";
        public override string KeyCopy => AnyInstanceReturnsThisForKeyCopy;

        public override bool Equals(ILookup? other)
        {
            return !base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return !base.Equals(obj);
        }
    }
}
