// // Exceptions.cs
// // Will Dresh
// // w@dresh.app
using System;
using static DBInterface.LookupManager;

namespace DBInterface
{
    public class DBInterfaceFatalException: Exception
    {
        private static readonly string DBIFMessage = "Fatal Exception (DBInterface)";
        internal DBInterfaceFatalException()
            : base(DBIFMessage) { }

        internal DBInterfaceFatalException(string message)
            : base(GenerateMessage(message)) { }

        internal DBInterfaceFatalException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        internal DBInterfaceFatalException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DBIFMessage, msg);
        }
    }

    /// <summary>
    /// DB Interface application exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class DBInterfaceApplicationException : ApplicationException
    {
        private static readonly string DBIMessage = "DBInterface Exception";
        internal DBInterfaceApplicationException()
            : base(DBIMessage) { }

        internal DBInterfaceApplicationException(string message)
            : base(GenerateMessage(message)) { }

        internal DBInterfaceApplicationException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        internal DBInterfaceApplicationException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DBIMessage, msg);
        }
    }

    internal class ExternalIntegrationException: DBInterfaceApplicationException
    {

        private static readonly string DBIMessage = "External Integration Exception";

        internal ExternalIntegrationException()
            : base(DBIMessage) { }

        internal ExternalIntegrationException(string message)
            : base(GenerateMessage(message)) { }

        internal ExternalIntegrationException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        internal ExternalIntegrationException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DBIMessage, msg);
        }
    }

    // TODO - Remove this exception class
    /// <summary>
    /// (WILL BE REMOVED SOON) Operation not permitted exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class OperationNotPermittedException : DBInterfaceApplicationException
    {
        private static readonly string DefaultMessage = "Operation not permitted";
        internal OperationNotPermittedException()
            : base(DefaultMessage) { }

        internal OperationNotPermittedException(string message)
            : base(GenerateMessage(message)) { }

        internal OperationNotPermittedException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DefaultMessage, msg);
        }
    }

    /// <summary>
    /// Security exception.
    /// </summary>
    /// <remarks>This class cannot be externally inherited, as it has
    /// no public constructor.</remarks>
    public class SecurityException : OperationNotPermittedException
    {
        private static readonly string SecurityMessage = "Application security violation";
        internal SecurityException()
            : base(SecurityMessage) { }

        internal SecurityException(string message)
            : base(GenerateMessage(message)) { }

        internal SecurityException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", SecurityMessage, msg);
        }
    }

    /// <summary>
    /// Data unreachable exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class DataUnreachableException : DBInterfaceApplicationException
    {
        private static readonly string DefaultMessage = "Data Unreachable";
        internal DataUnreachableException()
            : base(DefaultMessage) { }

        internal DataUnreachableException(string message)
            : base(GenerateMessage(message)) { }

        internal DataUnreachableException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        internal DataUnreachableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        private static string GenerateMessage(string msg)
        {
            return string.Format("{0}: {1}", DefaultMessage, msg);
        }
    }

    public class IllegalCacheOperationException : ApplicationException
    { }


    partial class DBLookupResult
    {
        /// <summary>
        /// DBLookupResult bug detected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="ArgumentNullException"/>.
        /// </remarks>
        internal sealed class DBLookupResult_BugDetectedException : ArgumentNullException
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
    partial class DBLookup
    {
        internal sealed class DBLookupBugDetectedException : ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of DBLookup; this may mean a bug in the caller's code";
            public DBLookupBugDetectedException(string argumentName)
                : base(argumentName, FBDEMessage) { }
        }
    }

    partial class DBLookupManager
    {
        internal sealed class UnexpectedBehaviorException: DBInterfaceFatalException
        {
            private static readonly string UBEMessage = "Unexpected behavior by a provider of System.Data.IDbConnection";

            public UnexpectedBehaviorException()
                : base(UBEMessage)
            { }

            public UnexpectedBehaviorException(string msg)
                :base(GenerateMessage(msg))
            { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", UBEMessage, msg);
            }
        }

        internal sealed class DBLookupManagerBugDetectedException : ArgumentNullException
        {
            private static readonly string FBDEMessage = "A required argument was null in a call to a static internal method of DBLookupManager; this probably means a bug in the caller's code";
            internal DBLookupManagerBugDetectedException(string methodName, string argumentName)
                : base(argumentName, FBDEMessage) { MethodName = methodName; }

            public string MethodName { get; }

            private static string GenerateMessage(string method, string argument)
            {
                return String.Format("{0} -- MethodName={1} -- ArgumentName={2}", FBDEMessage, method, argument);
            }
        }

        /// <summary>
        /// Internal instance expected exception.
        /// </summary>
        /// <remarks>
        /// These could be publicly caught as type <see cref="SecurityException"/>
        /// </remarks>
        internal sealed class InternalInstanceExpectedException : SecurityException
        {
            private static readonly string IIEEMessage = "Internal instance expected in a method of class DBLookupManager";

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

    abstract partial class LookupManager
    {

        /// <summary>
        /// Custom type failed verification exception. This class cannot be inherited.
        /// This class cannot be publicly constructed.
        /// </summary>
        protected internal sealed class CustomTypeFailedVerificationException : SecurityException
        {
            private static readonly string CTFVEMessage = "A custom mutable instance failed internal integrity checks";

            public External_IMutableLookup_VerificationFlags VerificationFlags { get; }

            internal CustomTypeFailedVerificationException(External_IMutableLookup_VerificationFlags flags)
                : base(CTFVEMessage)
            { VerificationFlags = flags; }
        }

        /// <summary>
        /// Lookup not permitted exception. This class cannot be inherited.
        /// </summary>
        protected internal sealed class LookupNotPermittedException : OperationNotPermittedException
        {
            private static readonly string LookupNotPermittedMessage = "Policy prohibits lookup operation";
            public LookupNotPermittedException()
                : base(LookupNotPermittedMessage) { }

            public LookupNotPermittedException(string message)
                : base(GenerateMessage(message)) { }

            public LookupNotPermittedException(string message, Exception innerException)
                : base(GenerateMessage(message), innerException) { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", LookupNotPermittedMessage, msg);
            }
        }
    }
}

namespace DBInterface.Cache
{
    partial class CacheFunctionProvider
    {
        public sealed class NullKeyException : IllegalCacheOperationException
        { }

        public sealed class NullQueryException : IllegalCacheOperationException
        { }
    }
}
