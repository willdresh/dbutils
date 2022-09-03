using DBInterface;
using System.Data;
using static DBInterface.DBManager;
using Dummy = DBInterface_XUnit_Tests.DummyDBConnection;

namespace DBInterface_XUnit_Tests
{
    public class DBManagerTests
    {

        internal static DbConnectionUpdated default_event_handler = (x, y) => (y as Dummy).testBit = true;
        internal static ObtainDbConnectionEventArgs static_args = new ObtainDbConnectionEventArgs();
        internal static DBConnectionProvider static_provider = (x) => Dummy.Build(x);
        internal static DBManager static_instance;

        internal static DBManager build_test_mgr(
            bool bind_BefDBCnx_Changes = false, bool bind_AftDBCnx_Changes = false
            )
        {
            DBManager result = DBManager.Build(static_provider);
            if (bind_BefDBCnx_Changes) result.BeforeDBConnectionChanges += default_event_handler;
            if (bind_AftDBCnx_Changes) result.AfterDBConnectionChanged += default_event_handler;


            return result;
        }

        static DBManagerTests()
        {
            try { static_instance = DBManager.Build(static_provider); }
            catch (ArgumentNullException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (NoNullAllowedException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (DBInterfaceFatalException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
        }

        internal class Test_Wrapper_DBManager
        {

        }

        public class Events
        {

            [Fact]
            public void BeforeDBConnectionChanges_Invoked_During_Call_To_NextConnection()
            {
                DBManager test = build_test_mgr(true);
                test.NextConnection();

                Assert.True(dummy.testBit);
            }

            [Fact]
            public void AfterDBConnectionChanges_Invoked_During_Call_To_NextConnection()
            {

            }
        }
    }
}
