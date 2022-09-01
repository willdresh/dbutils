// // CacheDBLookupFactory_Aux.cs
// // Will Dresh
// // w@dresh.app
using System;
using MutableVerificationFlags = DBInterface.LookupManager.External_IMutableLookup_VerificationFlags;

namespace DBInterface.CacheDB
{
    public static partial class CacheDBLookupFactory
    {
        /// <summary>
        /// Custom type failed verification exception. This class cannot be inherited.
        /// This class has no public constructor.
        /// </summary>
        public sealed class CustomTypeFailedVerificationException : SecurityException
        {
            private static readonly string CTFVEMessage = "A custom mutable instance failed internal integrity checks at the CacheDBLookupFactory";

            public MutableVerificationFlags VerificationFlags { get; }

            internal CustomTypeFailedVerificationException(MutableVerificationFlags flags)
                : base(CTFVEMessage)
            { VerificationFlags = flags; }
        }
    }
}
