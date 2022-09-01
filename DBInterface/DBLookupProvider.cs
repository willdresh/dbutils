// // DBLookupProvider.cs
// // Will Dresh
// // w@dresh.app
using System.Data;

namespace DBInterface
{
    public sealed class DBLookupProvider: ILookupProvider<DBLookupBase>
    {
        internal DBLookupProvider() { }

        internal DBLookupResult Lookup_Internal(DBLookupBase query)
        {
            var connection = query.DBConnection;
            IDbTransaction xaction = connection.BeginTransaction();
            IDbCommand command = connection.CreateCommand();
            command.Transaction = xaction;
            command.CommandText = query.ReadOnlyKey;
            IDataReader reader = command.ExecuteReader();
            DataTable result = new DataTable();
            result.Load(reader);

            return DBLookupResult.Build_Internal(query, result);
        }

        public ILookupResult<DBLookupBase> Lookup(DBLookupBase query)
        {
            return Lookup_Internal(query);
        }
    }
}
