// // DBLookup_Aux.cs
// // Will Dresh
// // w@dresh.app
using System;
namespace DBInterface
{
    internal partial class DBLookupResult
    {
        /// <summary>
        /// DBLookupResult bug detected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="ArgumentNullException"/>.
        /// </remarks>
        internal sealed class DBLookupResult_BugDetectedException: ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of DBLookupResult; this probably means a bug in the caller's code";
            public DBLookupResult_BugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }

        /// <summary>
        /// Internal instance expected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="SecurityException"/>
        /// </remarks>
        internal sealed class InternalInstanceExpectedException : SecurityException
        {
            private static readonly string IIEEMessage = "Internal DBLookupResult instance expected";

            internal InternalInstanceExpectedException()
                : base(IIEEMessage) { }

            internal InternalInstanceExpectedException(string message)
                : base(GenerateMessage(message)) { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", IIEEMessage, msg);
            }
        }
    }
}
