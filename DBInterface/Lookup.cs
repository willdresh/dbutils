﻿// // Lookup.cs
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
        /// Gets a copy of the lookup key, or returns null. Direct references to the
	    /// lookup key itself cannot be obtained.
        /// </summary>
        public string KeyCopy { get { return (key == null ? null : String.Copy(key)); } 
	       	internal set { key = value == null ? null : String.Copy(value); } }

        /// <summary>
        /// Implements deep value-equality between Lookup objects
        /// </summary>
        /// <returns><c>true</c> if <c>other</c> has a key which is value-equal to the key of the current
        /// <see cref="DBInterface.Lookup"/>; otherwise, <c>false</c>.</returns>
        public virtual bool Equals(ILookup other)
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

            // If neither is null, then compare equality
            return Key_Internal.Equals(otherKey);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, this) // Short-circuit if reference-equal
                  || Equals(obj as ILookup);
        }

        public override int GetHashCode()
        {
            int result = Hashcode_XOR_Operand;
            if (Key_Internal != null)
                result ^= Key_Internal.GetHashCode();

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
    public class MutableLookup: ILookup, IMutableLookup<ILookup>
    {
        internal sealed class MutableLookup_BugDetectedException: ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of MutableLookup; this probably means a bug in the caller's code";
            public MutableLookup_BugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }

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

	    internal Lookup Unwrap_Immutable { get { return lu; } }

        public string KeyCopy
        {
            get { return lu.KeyCopy; }
            set { lu.KeyCopy = value; }
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

        public bool Equals(ILookup other)
        {
            // alias Lookup.Equals
            return (ImmutableCopy() as Lookup).Equals(other);
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
