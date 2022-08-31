// // DBLookupBase.cs
// // Will Dresh
// // w@dresh.app
using System;
using System.Data;
namespace DBInterface
{
    /// <summary>
    /// <c>DBLookupBase</c> - primary function of this abstract class is to
    /// securely wrap a database connection (with internal-only access) and
    /// associate it with a <see cref="Lookup"/> object.
    /// </summary>
    /// <remarks>
    /// This class cannot be externally inherited, as it has no
    /// public constructors.
    /// <br /><br />
    /// Implements custom equality behavior by overriding <c>System.Object.Equals(object)</c>.
    /// </remarks>
    /// <seealso cref="DBLookupBase.Equals(object)"/>
    public abstract class DBLookupBase: Lookup, IEquatable<DBLookupBase>
    {
        private const int Hashcode_XOR_Operand = 2;

        internal DBLookupBase(string key, System.Data.IDbConnection dbConnection)
            :base(key)
        { DBConnection = dbConnection; }

        /// <summary>
        /// Gets or sets the DB Connection.
        /// </summary>
        /// <value>The DBConnection.</value>
        /// <remarks>Keeping the database connection internal is essential
        /// to the integrity of this assembly</remarks>
        internal IDbConnection DBConnection {
            // Do not make public!
            get; set;
        } 

        /// <summary>
        /// Gets the state of the DB Connection.
        /// </summary>
        /// <value>The state of the DB Connection.</value>
        /// <remarks>
        /// This property is provided with intent to allow for external
        /// assemblies to inspect the connection state; internal components
        /// should NOT depend on access to this property.
        /// </remarks>
        public System.Data.ConnectionState DBConnectionState
        {
            get { return DBConnection.State; }
        }

        /// <summary>
        /// Defines custom equality behavior between instances of <c>DBLookupBase</c>,
        /// such that two instances are equal IF AND ONLY IF their <see cref="DBLookupBase.DBConnection"/>
        /// properties are reference-equal AND they are equal according to base class <see cref="Lookup"/>.
        /// </summary>
        /// <param name="other">The <see cref="DBInterface.DBLookupBase"/> to compare with the current <see cref="DBInterface.DBLookupBase"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="DBInterface.DBLookupBase"/> is equal to the current
        /// <see cref="DBInterface.DBLookupBase"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(DBLookupBase other)
        {
            if (other == null) return false;
            return
                ReferenceEquals(other.DBConnection, this.DBConnection) // DBConnection property must be reference-equal
                && base.Equals(other as Lookup);                       // Must be equal according to base class (Lookup)
        }

        /// <summary>
        /// Implements the following custom equality behavior:<br />
        /// (1) IF <c>obj is DBLookupBase dblb</c>, THEN <c>return Equals(dblb)</c>(*)
        /// (2) ELSE <c>return base.Equals(obj)</c> (where <c>base</c> refers to <see cref="T:Lookup"/>)
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:DBInterface.DBLookupBase"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:DBInterface.DBLookupBase"/>; otherwise, <c>false</c>.</returns>
        /// <seealso cref="DBLookupBase.Equals(DBLookupBase)"/>
        /// <seealso cref="Lookup.Equals(Lookup)"/>
        /// <remarks>(*) before calling <c>this.Equals(dblb)</c>, short-circuit and return <c>true</c>
        /// if <c>obj</c> is reference-equal to <c>this</c>.</remarks>
        public override bool Equals(object obj)
        {
            if (obj is DBLookupBase dblb)
                return ReferenceEquals(obj, this) // Short-circuit if reference-equal
                    || Equals(dblb);

            // For non-DBLookupBase objects,
            // Call Lookup.Equals(obj) to check equality.
            // This custom behavior allows less-derived lookup types
            // to be said to be "equal to" a more-derived type
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode() ^ Hashcode_XOR_Operand;
            if (DBConnection != null)
                result ^= DBConnection.GetHashCode();
            return result;
        }
    }
}
