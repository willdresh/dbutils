using DBInterface;
using System.Data;
using Dummy = DBInterface_XUnit_Tests.DummyDBConnection;

namespace DBInterface_XUnit_Tests
{
    public class DBManagerTests
    {
        
        #region Static instances for use by any tests that do not need to modify member data

        internal static DBManager.DbConnectionUpdated_For_XUnit set_newcnx_testbit_true =
            (args) => { (args.NewCnx as Dummy).TestBit = true; };

        internal static DBConnectionProvider build_dummy = Dummy.Build;
        internal static DBManager build_test_mgr(IDbConnection dummy) { return DBManager.Build(() => dummy); }
        internal static DBManager static_mgr;
        #endregion


        static DBManagerTests()
        {
            try { static_mgr = build_test_mgr(build_dummy()); }
            catch (ArgumentNullException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (NoNullAllowedException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
            catch (DBInterfaceFatalException ex) { throw new ApplicationException("from DBManagerTests::static DBManagerTests()", ex); }
        }

        public class Events
        {

            [Fact]
            public void BeforeDBConnectionChange_Invoked_During_Call_To_NextConnection()
            {
                bool result;
                IDbConnection newCnx;
                Dummy dummy = build_dummy() as Dummy;
                DBManager test = build_test_mgr(dummy);
                test.XUnit_BefDBCnxCha += set_newcnx_testbit_true;

                test.NextConnection();
                newCnx = test.GetCnx_For_XUnit() as Dummy;


                Assert.IsType<Dummy>(test.GetCnx_For_XUnit()); // This assertion is a tautology based on how we've arranged

                                                                // If this assertion fails, then the test instance is actually
                Assert.True(ReferenceEquals(newCnx, dummy));    // generating multiple dummy instances, instead of always returning the same instance
                                                                // It may be a logical error in build_test_mgr

                                            // This is the real assertion we want to test:
                Assert.True(dummy.TestBit); // did the dummy get acted on by set_newcnx_testbit_true
                                            // after the call to NextConnection()?

            }
        }
    }
}
