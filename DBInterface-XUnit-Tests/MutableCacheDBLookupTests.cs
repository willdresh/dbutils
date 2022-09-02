using DBInterface;
using DBInterface.CacheDB;
using Moq;
using System.Data;
using System.Reflection.Metadata.Ecma335;

namespace DBInterface_XUnit_Tests
{
    // TODO
    /// <summary>
    /// Broken.
    /// </summary>
    public class MutableCacheDBLookupTests
    {
        internal static readonly CacheDBLookupManager NullConnectionManager = new CacheDBLookupManager(null);
        internal static readonly Func<string, ILookup> LookupBuilder = Lookup.Build;
        internal static readonly Func<CacheDBLookupManager, ILookup, ICacheLookup> CDBLBuilder = (mgr, query) => CacheDBLookupFactory.BuildLookup(mgr, query);
        internal static readonly Func<MutableCacheDBLookup> TestInstance = () => CacheDBLookupFactory.BuildMutableLookup(NullConnectionManager, Lookup.Build("CacheDBLookupTests - Default Lookup instance"));
        internal static readonly Func<DBLookupBase> TestCopy = () => TestInstance().ImmutableCopy();

        public class Mutability
        {
            public void ImmutableCopy_Not_Mutable()
            {
                Assert.False(TestCopy() is IMutableLookup<ILookup>);
            }
        }

        public class Equality
        {
            [Fact]
            public void ImmutableCopy_EqualTo_This()
            {
                Assert.True(TestCopy().Equals(TestInstance()));
            }

            [Fact]
            public void EqualTo_ImmutableCopy()
            {
                Assert.True(TestInstance().Equals(TestCopy()));
            }

            [Fact]
            public void EqualTo_Self()
            {
                Assert.True(TestInstance().Equals(TestInstance()));
            }
        }
    
    }
}
