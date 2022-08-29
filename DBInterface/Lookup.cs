// // Lookup.cs
// // Will Dresh
// // w@dresh.app

using System;

namespace DBInterface
{
    /// <summary>
    /// Lookup. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    public class Lookup: ILookup, IEquatable<Lookup>
    {
        private const int Hashcode_XOR_Operand = 17;

        internal Lookup(string _key)
        {
            Key = _key;
        }

        internal Lookup(MutableLookup lu)
        {
            Key = lu.Key;
        }

        public string Key { get; internal set; }

        /// <summary>
        /// Implements deep value-equality between Lookup objects
        /// </summary>
        /// <returns><c>true</c> if <c>other</c> has a key which is value-equal to the key of the current
        /// <see cref="T:DBInterface.Lookup"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Lookup other)
        {
            if (other == null) return false;
            if (other.Key == null) return this.Key == null;

            return other.Key.Equals(this.Key);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, this) || // Short-circuit if reference-equal
                  Equals(obj as Lookup);
        }

        public override int GetHashCode()
        {
            int result = Hashcode_XOR_Operand;
            if (Key != null)
                result ^= Key.GetHashCode();

            return result;
        }
    }

    public class MutableLookup: IMutableLookup, IEquatable<MutableLookup>
    {
        private const int Hashcode_XOR_Operand = 48;

        private readonly Lookup lu;

        internal MutableLookup(MutableLookup other)
        {
            lu = other.lu;
        }

        internal MutableLookup(string key = null)
        {
            lu = new Lookup(key);
        }

        public string Key
        {
            get { return lu.Key; }
            set { lu.Key = value; }
        }

        public static MutableLookup Copy_Internal(MutableLookup original)
        {
            return new MutableLookup(original);
        }

        public static IMutableLookup Copy(MutableLookup original)
        {
            return Copy_Internal(original);
        }

        internal static MutableLookup Build_Internal(string key)
        {
            return new MutableLookup(key);
        }

        public static IMutableLookup Build(string key)
        {
            return Build_Internal(key);
        }

        public bool Equals(MutableLookup other)
        {
            return this.AsImmutable().Equals(other.AsImmutable());
        }

        internal virtual Lookup AsImmutable_Internal()
        {
            return lu;
        }

        public ILookup AsImmutable()
        {
            return AsImmutable_Internal();
        }

        public override bool Equals(object obj)
        {
            MutableLookup other = obj as MutableLookup;
            if (obj == null || other == null) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return lu.GetHashCode() ^ Hashcode_XOR_Operand;
        }
    }

    internal class LookupResult: ILookupResult
    {
        internal LookupResult(ILookup _query, System.Object _result)
        {
            Query = _query;
            Response = _result;
        }

        public ILookup Query { get; }
        public object Response { get; }
    }
}
