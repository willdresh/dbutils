using DBInterface;

namespace DBInterface_XUnit_Tests.ExternalTypes
{
    public class Integration_ExternalLookup
    {
        private static readonly Func<string?, ExternalLookup> GetTestInstance = (string? k) => new ExternalLookup(k);
        private static readonly Func<string?, BadExternalLookup> GetBadTestInstance = (string? k) => new BadExternalLookup(k);

        [Fact]
        public void BadType_KeyCopy_NotEqual_Original_Key()
        {
            string testKey = "test";
            ILookup test = GetBadTestInstance(testKey);

            Assert.False(test.KeyCopy.Equals(testKey));
        }

        [Fact]
        public void GoodType_KeyCopy_Equals_Original_Key()
        {
            string testKey = "test";
            ILookup test = GetTestInstance(testKey);

            Assert.True(test.KeyCopy.Equals(testKey));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData(null)]
        public void GoodType_Equals_SameKey(string? testKey)
        {
            ILookup test1 = GetTestInstance(testKey),
                test2 = GetTestInstance(testKey != null ? String.Copy(testKey) : null);
            Assert.True(test1.Equals(test2) && test2.Equals(test1));
        }

        [Theory]
        [InlineData("LookupTests_ExternalTypes::Good TestKey")]
        [InlineData(null)]
        public void GoodType_Equals_Self(string? testKey)
        {
            ILookup lookup = GetTestInstance(testKey);
            Assert.True(lookup.Equals(lookup));
        }

        [Theory]
        [InlineData("LookupTests_ExternalTypes::Bad TestKey")]
        [InlineData(null)]
        public void BadType_NotEqual_Self(string? testKey)
        {
            ILookup test = GetBadTestInstance(testKey);
            Assert.False(test.Equals(test));
        }

        [Theory]
        [InlineData("LookupTests_ExternalTypes::Bad TestKey")]
        [InlineData(null)]
        public void BadType_NotEqual_SameKey(string? testKey)
        {
            BadExternalLookup test1 = GetBadTestInstance(testKey);
            ExternalLookup test2 = GetTestInstance(testKey != null ? String.Copy(testKey!) : null),
                test3 = GetTestInstance(testKey != null ? String.Copy(testKey!) : null);

            Assert.False(test1.Equals(test2));
            Assert.False(test1.Equals(test3));
        }
    }

}
