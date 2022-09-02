using DBInterface;
using System;

namespace DBInterface_XUnit_Tests
{
    public class MutableLookupTests
    {
        private static readonly Func<string, MutableLookup> MutableLookupBuilder = MutableLookup.Build;
        private static readonly Func<string, Lookup> LookupBuilder = Lookup.Build;

        [Theory]
        [InlineData("Test Key: Equals_ImmutableCopy_Equals_Original")]
        [InlineData(null)]
        public void Equals_ImmutableCopy_Equals_Original(string? key)
        {
            MutableLookup test = MutableLookup.Build(key);

            Assert.True(test.Equals(test.ImmutableCopy()));
        }

        [Fact]
        public void Build_KeyCopy_ValueEqualTo_Build_Parameter()
        {
            string testKey = "Test Key: MutableLookup.Build returns an instance whose KeyCopy is value-equal (String.Equals) to the parameter";
            MutableLookup test = MutableLookup.Build(testKey);

            Assert.True(testKey.Equals(test.KeyCopy));
        }

        [Fact]
        public void Build_KeyCopy_NotReferenceEqualTo_Build_Parameter()
        {
            string testKey = "Test Key: MutableLookup.Build returns an instance whose KeyCopy is NOT reference-equal to the parameter";
            MutableLookup test = MutableLookup.Build(testKey);

            Assert.False(ReferenceEquals(testKey, test.KeyCopy));
        }
    }
}
