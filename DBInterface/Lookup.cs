// Lookup.cs
// Will Dresh
// w@dresh.app

using System;

namespace DBInterface
{
    /// <summary>
    /// Represents a lookup operation, which has a read-only string key.
    /// Instances of Lookup own the only reference to their key, and this reference is never
    /// exposed, even internally - thus providing immutability.
    /// This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    public class Lookup: ILookup
    {
        private string key;

        internal Lookup(Lookup other)
        {
            KeyCopy = other.key;
        }

        internal Lookup(string _key)
        {
            KeyCopy = _key;
        }

        internal Lookup(MutableLookup lu)
        {
            KeyCopy = lu.Unwrap_Immutable.ReadOnlyKey;
        }

        /// <summary>
        /// This reference should never be transformed or externally exposed!
        /// Ideally this property should be made private or removed entirely in future versions.
        /// </summary>
        //TODO
	    internal string ReadOnlyKey { get { return key; } }

        /// <summary>
        /// Gets a copy of the lookup key, or returns null (if key is null). References to the
	    /// lookup key itself cannot be obtained externally. <br />
        /// </summary>
        /// <remarks>
        /// (Internal-only) Set accessor automatically executes a copy operation before storing the supplied <c>value</c>
        /// </remarks>
        public string KeyCopy { get { return (key == null ? null : String.Copy(key)); } 
	       	internal set {
                key = value == null ? null : String.Copy(value); }
        }

        /// <summary>
        /// Implements deep value-equality between Lookup objects
        /// </summary>
        /// <returns><c>true</c> if <c>other</c> has a key which is value-equal to the key of the current
        /// <see cref="Lookup"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(ILookup other)
        {
            if (other == null) return false;

    	    string otherKey;

    	    // Apply Optimization:
    	    // If we are comparing to an internally-defined type (either Lookup or MutableLookup)
    	    // then we can avoid unnecessary key copying operations by using the internally-accessible
    	    // properties Unwrap_Immutable and Key_Internal.
    	    if (other is Lookup int_lu) 
    		    otherKey = int_lu.ReadOnlyKey;
    	    else if (other is MutableLookup mu) 
    		    otherKey = mu.Unwrap_Immutable.ReadOnlyKey;
    	    else otherKey = other.KeyCopy; // If we cannot infer a concrete type, then we'll have to use KeyCopy in order to compare keys.

            // If at least one is null, then return "are they both null?"
            if (ReadOnlyKey == null || otherKey == null)
                return (ReadOnlyKey == null) && (otherKey == null);

            // If neither is null, then compare key value-equality
            return ReadOnlyKey.Equals(otherKey);
        }

        /// <summary>
        /// Build a <see cref="Lookup"/> instance.
        /// </summary>
        /// <param name="key">
        /// (nullable) The lookup key, which will be COPIED into the instance returned by this method.
        /// </param>
        /// <returns>A newly-created instance of <see cref="Lookup"/> whose key is identical (String.Equals) to the supplied key</returns>
        public static Lookup Build(string key = null)
        {
            return new Lookup(key);
        }

        public static Lookup BuildCopy(Lookup other)
        {
            return new Lookup(other);
        }

        public static Lookup BuildCopy(MutableLookup mutableOther)
        {
            return new Lookup(mutableOther.Unwrap_Immutable);
        }
    }

    /// <summary>
    /// Mutable lookup.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no
    /// public constructor.
    /// </remarks>
    public class MutableLookup: ILookup, IMutableLookup<ILookup>, IEquatable<MutableLookup>
    {
        private const int Hashcode_XOR_Operand = 48;

        private readonly Lookup lu;
        private string readOnlyKey;

        internal MutableLookup(MutableLookup other)
        {
            lu = new Lookup(other.lu);
            readOnlyKey = lu.KeyCopy;
        }

        internal MutableLookup(string key = null)
        {
            lu = new Lookup(key);
            readOnlyKey = key;
	    }

	    internal Lookup Unwrap_Immutable { get { return lu; } }

        /// <summary>
        /// Get or set the lookup key for this <see cref="MutableLookup"/>. <br />
        /// This accessor differs slightly from <see cref="MutableLookup.KeyCopy"/>, in that
        /// when <c>MutableLookup.Key.get</c> is called, no new copy of the key is generated. <br />
        /// This accessor should generally be preferred over <see cref="MutableLookup.KeyCopy.get"/>
        /// wherever possible for best performance.<br />
        /// </summary>
        /// <remarks>
        /// The reference exposed by this method 
        /// </remarks>
        public string GetReadOnlyKey() {
            return readOnlyKey;
        }

        /// <summary>
        /// Get or set a copy of the lookup key.<br />
        /// As a rule, <see cref="ILookup"/>s own the only reference to their key, and they <br />
        /// never expose this reference - not even internally.
        /// Thus, every call to <c>MutableLookup.KeyCopy</c> - be it a <c>get</c> or a <c>set</c> -
        /// results in a call to <see cref="String.Copy(string)"/>.<br />
        /// <br />
        /// To improve performance, <see cref="MutableLookup.GetReadOnlyKey()" /> may be used instead of
        /// <c>MutableLookup.KeyCopy.get</c> in order to avoid a call to <c>String.Copy(string)</c>.
        /// </summary>
        public string KeyCopy
        {
            get => lu.KeyCopy;
            set => lu.KeyCopy = readOnlyKey = value;
        }

        public static MutableLookup Copy(MutableLookup original)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            
            return new MutableLookup(original);
        }

        /// <summary>
        /// Copies
        /// </summary>
        /// <param name="original">(NOT NULL) Original</param>
        /// <returns>A newly-constructed copy</returns>
        /// <exception cref="ArgumentNullException"><c>original</c> is <c>null</c>.</exception>
        public static MutableLookup BuildCopy(ILookup original)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            return new MutableLookup(original.KeyCopy);
        }

        /// <summary>
        /// Builds an instance. Exception-safe.
        /// </summary>
        /// <returns>A newly-constructed instance.</returns>
        /// <param name="key">(nullable) Key.</param>
        public static MutableLookup Build(string key=null)
        {
            return new MutableLookup(key);
        }

        /// <summary>
        /// Construct and return a new immutable copy of this
        /// instance. Exception-safe.
        /// </summary>
        /// <returns>The copy.</returns>
        public ILookup ImmutableCopy()
        {
            return new Lookup(this);
        }

        /// <summary>
        /// calls Lookup.Equals
        /// </summary>
        /// <param name="other">The <see cref="DBInterface.ILookup"/> to compare with the current <see cref="T:DBInterface.MutableLookup"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="DBInterface.ILookup"/> is equal to the current
        /// <see cref="T:DBInterface.MutableLookup"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><c>other</c> is <c>null</c>.</exception>
        public bool Equals(ILookup other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // alias Lookup.Equals
            return lu.Equals(other);
        }

        /// <summary>
        /// Check if two instances are value-equal
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns><c>true</c> if the two instances are value-equal; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><c>other</c> is <c>null</c>.</exception>
        public bool Equals(MutableLookup other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            string origKey = lu.ReadOnlyKey,
                copyKey = other.lu.ReadOnlyKey;

            if (origKey == null)
                return copyKey == null;

            return origKey.Equals(copyKey);
        }
    }

    internal class LookupResult: ILookupResult<ILookup>
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
