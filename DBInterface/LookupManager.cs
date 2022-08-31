// // LookupManager.cs
// // Will Dresh
// // w@dresh.app

using System;
namespace DBInterface
{
    /// <summary>
    /// Lookup Manager. This class cannot be externally inherited, as it has no
    /// public constructors.
    /// </summary>
    public abstract class LookupManager: ILookupProvider
    {
        /// <summary>
        /// Lookup not permitted exception. This class cannot be externally inherited, as it has no
        /// public constructors.
        /// </summary>
        public class LookupNotPermittedException: OperationNotPermittedException
        {
            private static readonly string LookupNotPermittedMessage = "Policy prohibits lookup operation";
            internal LookupNotPermittedException()
                : base(LookupNotPermittedMessage) { }

            internal LookupNotPermittedException(string message)
                : base(GenerateMessage(message)) { }

            internal LookupNotPermittedException(string message, Exception innerException)
                : base(GenerateMessage(message), innerException) { }

            private static string GenerateMessage(string msg)
            {
                return String.Format("{0}: {1}", LookupNotPermittedMessage, msg);
            }
        }

        [Flags]
        public enum LookupPolicy { ALLOW_LOOKUP = 1 }
        public const LookupPolicy DEFAULT_Policy = LookupPolicy.ALLOW_LOOKUP;

        public LookupPolicy Policy { get; }

        internal LookupManager(LookupPolicy policy = DEFAULT_Policy)
        {
            Policy = DEFAULT_Policy;
        }

        /// <summary>
        /// Does the lookup.
        /// </summary>
        /// <returns>The lookup.</returns>
        /// <param name="query">Query (LookupManager guarantees that the runtime type of <c>query</c>
        ///  will be immutable</param>
        protected abstract ILookupResult DoLookup(ILookup query);

        public virtual bool LookupAllowed { get => Policy.HasFlag(LookupPolicy.ALLOW_LOOKUP); }

        /// <summary>
        /// Lookup the specified query.
        /// </summary>
        /// <returns>The lookup.</returns>
        /// <param name="query">Query.</param>
        /// <exception cref="LookupNotPermittedException">Lookup is not allowed</exception>
        public ILookupResult Lookup(ILookup query)
        {
            if (LookupAllowed)
            {
                if (query is IMutableLookup mutable)
                    return DoLookup(mutable.ImmutableCopy());
                else
                    return DoLookup(query);
            }
            else throw new LookupNotPermittedException();
        }
    }
}
