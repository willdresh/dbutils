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
        private string key;

        internal Lookup(string _key)
        {
            KeyCopy = _key;
        }

        internal Lookup(MutableLookup lu)
        {
            KeyCopy = lu.Unwrap_Immutable.Key_Internal;
        }

	    internal string Key_Internal { get { return key; } set { key = value; } }

        /// <summary>
        /// Gets a copy of the lookup key, or returns null (if key is null). References to the
	    /// lookup key itself cannot be obtained externally.
        /// </summary>
        public string KeyCopy { get { return (key == null ? null : String.Copy(key)); } 
	       	internal set { key = value == null ? null : String.Copy(value); } }

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
    		    otherKey = int_lu.Key_Internal;
    	    else if (other is MutableLookup mu) 
    		    otherKey = mu.Unwrap_Immutable.Key_Internal;
    	    else otherKey = other.KeyCopy; // If we cannot infer a concrete type, then we'll have to use KeyCopy in order to compare keys.

            // If at least one is null, then return "are they both null?"
            if (Key_Internal == null || otherKey == null)
                return (Key_Internal == null) && (otherKey == null);

            // If neither is null, then compare key value-equality
            return Key_Internal.Equals(otherKey);
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
            lu = other.lu;
            readOnlyKey = lu.KeyCopy;
        }

        internal MutableLookup(string key = null)
        {
            lu = new Lookup(key);
            readOnlyKey = lu.KeyCopy;
	    }

	    internal Lookup Unwrap_Immutable { get { return lu; } }

        public string KeyCopy
        {
            get { return readOnlyKey; }
            set { lu.KeyCopy = readOnlyKey = value; }
        }

        /// <summary>
        /// Copies
        /// </summary>
        /// <param name="original">(NOT NULL) Original</param>
        /// <returns>A newly-constructed copy</returns>
        /// <exception cref="ArgumentNullException"><c>original</c> is <c>null</c>.</exception>
        public static MutableLookup Copy(ILookup original)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            return new MutableLookup(original.KeyCopy);
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
        public static IMutableLookup<ILookup> Build(string key=null)
        {
            return Build_Internal(key);
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
        public bool Equals(ILookup other)
        {
            // alias Lookup.Equals
            return lu.Equals(other);
        }

        public bool Equals(MutableLookup other)
        {
            return lu.Key_Internal.Equals(other.lu.Key_Internal);
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
