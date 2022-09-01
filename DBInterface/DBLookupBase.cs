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
    /// </remarks>
    public abstract class DBLookupBase: Lookup, IEquatable<DBLookupBase>
    {
        internal DBLookupBase(string key)
            :base(key)
        { }

        /// <summary>
        /// Gets or sets the DB Connection.
        /// </summary>
        /// <value>The DBConnection.</value>
        /// <remarks>Keeping the database connection internal is essential
        /// to the integrity of this assembly</remarks>
        internal abstract IDbConnection DBConnection { get; set; }

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
    }
}
