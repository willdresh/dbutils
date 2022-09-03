using DBInterface;
using System.Data;

namespace DBInterface_XUnit_Tests
{
    /// <summary>
    /// Dummy provider of IDbConnection. Its interface members throw NotImplementedExceptions.
    /// </summary>
    internal class DummyDBConnection : IDbConnection
    {
        private static UInt16 auto_id_source = 1;
        private int auto_id;

        public bool? TestBit { get; set; }

        public DummyDBConnection()
        {
            TestBit = null;
            auto_id = auto_id_source++;
        }

        public override string ToString()
        {
            return $"DBInterface_XUnit_Tests::DummyDBConnection (id={auto_id})";
        }

        public static DummyDBConnection Build()
        {
            return new DummyDBConnection();
        }


        // Presently all members of IDbConnection throw a NotImplementedException
        #region IDbConnection members
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ConnectionTimeout => throw new NotImplementedException();

        public string Database => throw new NotImplementedException();

        public ConnectionState State => throw new NotImplementedException();

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}