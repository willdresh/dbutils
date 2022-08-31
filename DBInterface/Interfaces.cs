

namespace DBInterface
{
    public interface ILookup: System.IEquatable<ILookup>
    {
        string Key { get; }
    }

    public interface IMutableLookup<T>: ILookup
        where T: class, ILookup
    {
        /// <summary>
        /// Get an immutable copy of this mutable object
        /// </summary>
        /// <returns>A newly-constructed immutable copy of <c>this</c>.</returns>
        T ImmutableCopy();
    }

    public interface IMutableLookup: IMutableLookup<ILookup> { }

    public interface ILookupResult<T>
        where T: ILookup
    {
        T Query { get; }
        System.Object Response { get; }
    }

    public interface ILookupResult: ILookupResult<ILookup> { }

    internal interface ILookupProvider<T>
        where T: ILookup
    {
        ILookupResult Lookup(T query);
    }

    internal interface ILookupProvider: ILookupProvider<ILookup> {  }

}
