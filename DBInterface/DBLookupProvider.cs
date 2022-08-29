// // DBLookupProvider.cs
// // Will Dresh
// // w@dresh.app
using System.Data;

namespace DBInterface
{
    public sealed class DBLookupProvider : ILookupProvider<DBLookup>
    {
        public ILookupResult Lookup(DBLookup query)
        {
            var connection = query.DBConnection;
            IDbTransaction xaction = connection.BeginTransaction();
            IDbCommand command = connection.CreateCommand();
            command.Transaction = xaction;
            command.CommandText = query.Key;
            IDataReader reader = command.ExecuteReader();
            DataTable result = new DataTable();
            result.Load(reader);

            return new DBLookupResult(query, result);
        }
    }
}
