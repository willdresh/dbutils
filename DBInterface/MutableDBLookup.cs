// // MutableDBLookup.cs
// // Will Dresh
// // w@dresh.app
using System;

using VerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;
namespace DBInterface
{
    /// <summary>
    /// Mutable wrapper for <see cref="DBLookup"/>. This class cannot be
    /// externally inherited, as it has no public constructors.
    /// </summary>
    public sealed partial class MutableDBLookup:
        IMutableLookup<DBLookupBase>, IMutableLookup<ILookup>, IEquatable<MutableDBLookup>,
        ILookup
    {
        /// <summary>
        /// Custom type failed verification exception. This class cannot be inherited.
        /// This class cannot be publicly constructed.
        /// </summary>
        public sealed class CustomTypeFailedVerificationException : SecurityException
        {
            private static readonly string CTFVEMessage = "A custom mutable instance failed internal integrity checks in MutableDBLookup";

            public VerificationFlags VerificationFlags { get; }
            public object Instance { get; }

            internal CustomTypeFailedVerificationException(string typeDescription, object instance, VerificationFlags flags)
                : base(GenerateMessage(typeDescription, instance))
            {
                VerificationFlags = flags;
                Instance = instance;
            }

            internal CustomTypeFailedVerificationException(VerificationFlags flags)
                : base(CTFVEMessage)
            { VerificationFlags = flags; }

            private static string GenerateMessage(string typeDescription, object instance)
            {
                if (typeDescription == null || instance == null) return String.Format("(unexpected: null parameter) {0}", CTFVEMessage);
                return String.Format("{0} --- Custom description of type is {1} --- instance.ToString(): {2}", CTFVEMessage, typeDescription, instance.ToString());
            }
        }

        private DBLookup dbl;

        /// <summary>
        /// Gets a reference to the immutable instance wrapped by the current MutableDBLookup.
        /// Use of this reference can prevent the need for additional copying, but this reference must
        /// never be publicly accessible. Thus, to ensure integrity, only internal components may
        /// benefit from this optimization.
        /// </summary>
        /// <value>A reference to the immutable instance which is wrapped by the current MutableDBLookup.</value>
        /// <remarks>This should be considered a READ-ONLY reference (do not modify its data!)</remarks>
        internal DBLookup Unwrap_Immutable { get => dbl; }

        internal MutableDBLookup(string key = null, System.Data.IDbConnection dbConnection = null)
        {
            dbl = new DBLookup(key, dbConnection);
        }

        // DBConnection must never be made public to ensure integrity!
        private System.Data.IDbConnection DBConnection
        {
            get => dbl.DBConnection;
            set => dbl.DBConnection = value;
        }

        internal string Key_Internal { get => dbl.Key_Internal; set => dbl.Key_Internal = value; }
        public string KeyCopy { get => dbl.KeyCopy; set => dbl.KeyCopy = value; }

    }
}
