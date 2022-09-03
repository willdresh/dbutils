using DBInterface;
using System.Data;
using static DBInterface.DBManager;
using Dummy = DBInterface_XUnit_Tests.DummyDBConnection;

namespace DBInterface_XUnit_Tests
{
    public class DBManagerTests
    {
        
        #region Static instances for use by any tests that do not need to modify member data
        internal static DbConnectionUpdated_For_XUnit default_event_handler = (args) => { (args.NewCnx as Dummy).TestBit = true; return args; };
        internal static DBConnectionProvider build_dummy = () => Dummy.Build();
        internal static DBManager build_test_mgr() { return Build(build_dummy); }
        internal static DBManager static_instance;
        #endregion


        static DBManagerTests()
        {
            try { static_instance = DBManager.Build(build_dummy); }
            catch (ArgumentNullException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (NoNullAllowedException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (DBInterfaceFatalException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
        }

        public class Events
        {

            [Fact]
            public void DBConnectionChanges_Invoked_During_Call_To_NextConnection()
            {
                DBManager test = build_test_mgr();
                test.NextConnection();

                if (!(test.GetCnx_For_XUnit() is Dummy dummy))
                    throw new Exception("Not a dummy! DBManagerTests.cs:37");

                Assert.True(dummy.TestBit);
            }
        }
    }
}
