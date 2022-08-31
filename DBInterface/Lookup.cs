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
    public class Lookup: ILookup
    {
        private const int Hashcode_XOR_Operand = 17;

        internal Lookup(string _key)
        {
            Key = _key == null ? null : String.Copy(_key);
        }

        internal Lookup(MutableLookup lu)
        {
            Key = lu.Key == null ? null : String.Copy(lu.Key);
        }

        public string Key { get; internal set; }

        /// <summary>
        /// Implements deep value-equality between Lookup objects
        /// </summary>
        /// <returns><c>true</c> if <c>other</c> has a key which is value-equal to the key of the current
        /// <see cref="DBInterface.Lookup"/>; otherwise, <c>false</c>.</returns>
        public virtual bool Equals(ILookup other)
        {
            if (other == null) return false;
            if (other.Key == null) return this.Key == null;

            return other.Key.Equals(this.Key);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, this) // Short-circuit if reference-equal
                  || Equals(obj as ILookup);
        }

        public override int GetHashCode()
        {
            int result = Hashcode_XOR_Operand;
            if (Key != null)
                result ^= Key.GetHashCode();

            return result;
        }
    }

    /// <summary>
    /// Mutable lookup.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no
    /// public constructor.
    /// </remarks>
    public class MutableLookup: IMutableLookup
    {
        internal sealed class MutableLookup_BugDetectedException: ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of MutableLookup; this probably means a bug in the caller's code";
            public MutableLookup_BugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }

        private const int Hashcode_XOR_Operand = 48;

        private readonly Lookup lu;

        protected event Action<string> BeforeKeySet;

        internal MutableLookup(MutableLookup other)
        {
            lu = other.lu;
            BeforeKeySet += (obj) => {; }; // Prevent event from being subscriberless  
                                            // (events HATE that sort of thing, you know)
        }

        internal MutableLookup(string key = null)
        {
            if (key != null)
                lu = new Lookup(String.Copy(key));
            else lu = new Lookup(null as string);

            BeforeKeySet += (obj) => {; }; // Prevent event from being subscriberless  
                                           // (events HATE that sort of thing, you know)
        }

        public string Key
        {
            get { return lu.Key; }
            set { BeforeKeySet.Invoke(value); lu.Key = value; }
        }

        /// <summary>
        /// Copies (internal use only)
        /// </summary>
        /// <param name="original">(NOT NULL) Original</param>
        /// <returns>A newly-constructed copy</returns>
        /// <exception cref="MutableLookup_BugDetectedException"><c>original</c> is <c>null</c>.</exception>
        internal static MutableLookup Copy_Internal(MutableLookup original)
        {
            if (original == null)
                throw new MutableLookup_BugDetectedException(nameof(original));

            return new MutableLookup(original);
        }

        /// <summary>
        /// Builds an instance. Exception-safe. (Internal use only)
        /// </summary>
        /// <returns>A newly-constructed instance.</returns>
        /// <param name="key">(nullable) Key.</param>
        internal static MutableLookup Build_Internal(string key=null)
        {
            return new MutableLookup(key);
        }

        /// <summary>
        /// Builds an instance. Exception-safe.
        /// </summary>
        /// <returns>A newly-constructed instance.</returns>
        /// <param name="key">(nullable) Key.</param>
        public static IMutableLookup Build(string key=null)
        {
            return Build_Internal(key);
        }

        /// <summary>
        /// Construct and return a new immutable copy of this
        /// instance. Exception-safe.
        /// </summary>
        /// <returns>The copy.</returns>
        public virtual ILookup ImmutableCopy()
        {
            return new Lookup(this);
        }

        public virtual bool Equals(ILookup other)
        {
            if (other.Key == null || Key == null)
                return (other.Key == null) ^ (Key == null);
            return other.Key.Equals(this.Key);
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
