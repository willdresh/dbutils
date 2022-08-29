

namespace DBInterface
{
    public interface ILookup
    {
        string Key { get; }
    }

    public interface IMutableLookup<T>: ILookup
        where T : class, ILookup
    {
        T AsImmutable();
    }

    public interface IMutableLookup: IMutableLookup<ILookup> { }

    public interface ILookupResult<T>
        where T: ILookup
    {
        T Query { get; }
        System.Object Response { get; }
    }

    public interface ILookupResult: ILookupResult<ILookup> { }

    public interface ILookupProvider<T>
        where T: ILookup
    {
        ILookupResult Lookup(T query);
    }

    public interface ILookupProvider: ILookupProvider<ILookup> {  }

}
