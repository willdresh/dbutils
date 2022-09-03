using DBInterface;
using Moq;
using System;

namespace DBInterface_XUnit_Tests
{
    public class DBManagerTests
    {
        public class Events
        {
            private static DBManager static_test_instance = new DBManager();
            

            public class BeforeDBConnectionChanged
            {

                public void Not_Throw_Exception()
                {

                }
            }
        }
    }
}
