// // DBLookupProvider.cs
// // Will Dresh
// // w@dresh.app
using System.Data;

namespace DBInterface
{
    internal sealed class DBLookupProvider: ILookupProvider<DBLookup>
    {
        internal DBLookupProvider() { }

        internal DBLookupResult Lookup_Internal(DBLookup query)
        {
            var connection = query.DBConnection;
            IDbTransaction xaction = connection.BeginTransaction();
            IDbCommand command = connection.CreateCommand();
            command.Transaction = xaction;
            command.CommandText = query.Key;
            IDataReader reader = command.ExecuteReader();
            DataTable result = new DataTable();
            result.Load(reader);

            return DBLookupResult.Build_Internal(query, result);
        }

        public ILookupResult Lookup(DBLookup query)
        {
            return Lookup_Internal(query);
        }
    }
}
