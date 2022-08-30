// // Exceptions.cs
// // Will Dresh
// // w@dresh.app
using System;

namespace DBInterface
{
    /// <summary>
    /// DB Interface application exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class DBInterfaceApplicationException: ApplicationException
    {
        private static readonly string DBIMessage = "DBInterface Exception";
        internal DBInterfaceApplicationException()
            : base(DBIMessage) { }

        internal DBInterfaceApplicationException(string message)
            : base(GenerateMessage(message)) { }

        internal DBInterfaceApplicationException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DBIMessage, msg);
        }
    }

    /// <summary>
    /// Operation not permitted exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class OperationNotPermittedException: DBInterfaceApplicationException
    {
        private static readonly string DefaultMessage = "Operation not permitted";
        internal OperationNotPermittedException()
            :base(DefaultMessage) {  }

        internal OperationNotPermittedException(string message)
            : base(GenerateMessage(message)) { }

        internal OperationNotPermittedException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        private static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DefaultMessage, msg);
        }
    }

    public sealed class PolicyProhibitsAutoConnectException: DataUnreachableException
    {
        private static readonly string DefaultMessage = "Connection policy prohibits auto-connect";

        internal PolicyProhibitsAutoConnectException()
            : base(DefaultMessage) { }

        internal PolicyProhibitsAutoConnectException(string message)
            : base(GenerateMessage(message)) { }

        internal PolicyProhibitsAutoConnectException(string message, Exception innerException)
            : base(GenerateMessage(message), innerException) { }

        protected static string GenerateMessage(string msg)
        {
            return String.Format("{0}: {1}", DefaultMessage, msg);
        }
    }

    /// <summary>
    /// Data unreachable exception. Cannot be externally inherited, as it has
    /// no public constructor.
    /// </summary>
    public class DataUnreachableException : ApplicationException
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
}
