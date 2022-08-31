
namespace DBInterface
{
    public interface ILookup
    {
        string KeyCopy { get; }
    }

    public interface ICacheLookup: ILookup
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.ILookup"/> bypass cache.
        /// </summary>
        /// <value><c>true</c> if bypass cache; otherwise, <c>false</c>.</value>
        bool BypassCache { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:DBInterface.ICacheLookup"/> dont cache result.
        /// </summary>
        /// <value><c>true</c> if dont cache result; otherwise, <c>false</c>.</value>
        bool DontCacheResult { get; }
    }

    public interface IDBLookup: ILookup
    {
        /// <summary>
        /// Gets the DB Connection
        /// </summary>
        /// <value>The DBConnection</value>
        System.Data.IDbConnection DBConnection { get; }
    }

    /// <summary>
    /// Simply an alias for the intersection of ICacheLookup and IDBLookup
    /// </summary>
    public interface ICacheDBLookup : ICacheLookup, IDBLookup { }

    public interface ILookupResult<T>
        where T: ILookup
    {
        T Query { get; }
        System.Object Result { get; }
    }

    public interface ILookupResult: ILookupResult<ILookup> { }

    public interface ILookupProvider<T>
        where T: ILookup
    {
        ILookupResult<T> Lookup(T query);
    }
}
