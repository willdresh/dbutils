﻿using DBInterface;
using DBInterface.CacheDB;
using System.Data;

namespace DBInterface_XUnit_Tests
{
    public class CacheDBLookupTests
    {
        internal readonly CacheDBLookupManager testManager = new CacheDBLookupManager(null);
        internal static readonly Func<string, Lookup> LookupBuilder = Lookup.Build;
        internal static readonly Func<CacheDBLookupManager, ILookup, bool, bool, ICacheLookup> CDBLBuilder = CacheDBLookupFactory.BuildLookup;
        internal static readonly ILookup NullLookup = null;



        /// <summary>
        /// This should be moved to CacheDBLookupFactoryTests
        /// </summary>
        [Fact]
        public void CDBLBuilder_NullManager_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => CacheDBLookupFactory.BuildLookup(null, NullLookup));
        }
    
    }
}
