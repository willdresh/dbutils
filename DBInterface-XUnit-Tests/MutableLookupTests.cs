using DBInterface;
using System;

namespace DBInterface_XUnit_Tests
{
    public class MutableLookupTests
    {
        private static readonly Func<string, MutableLookup> MutableLookupBuilder = MutableLookup.Build;
        private static readonly Func<string, Lookup> LookupBuilder = Lookup.Build;

        [Fact]
        public void Build_KeyCopies_ValueEqual()
        {
            string testKey = "test Build_KeyCopies_ValueEqual (String.Equals)";
            string test1 = MutableLookupBuilder(testKey).KeyCopy,
                test2 = MutableLookupBuilder(testKey).KeyCopy;

            Assert.True(test1.Equals(test2));
        }

        [Fact]
        public void Build_KeyCopies_Not_ReferenceEqual()
        {
            string testKey = "test Build_KeyCopies_Not_ReferenceEqual";
            MutableLookup test1 = MutableLookupBuilder(testKey),
                test2 = MutableLookupBuilder(testKey);

            Assert.False(ReferenceEquals(test1.KeyCopy, testKey));
            Assert.False(ReferenceEquals(test1.KeyCopy, test2.KeyCopy));
        }

        [Theory]
        [InlineData("test Equals_Implements_ValueEquality_With_MutableLookup")]
        [InlineData(null)]
        public void Equals_Implements_ValueEquality_With_MutableLookup(string testKey)
        {
            MutableLookup test1 = MutableLookupBuilder(testKey),
                test2 = MutableLookupBuilder(testKey);

            Assert.True(test1.Equals(test2));
            Assert.True(test2.Equals(test1));
        }

        [Fact]
        public void ImmutableCopy_Returns_Not_InstanceOf_IMutableLookup_of_ILookup()
        {
            string testKey = "test";
            MutableLookup original = MutableLookupBuilder(testKey);
            ILookup copy = original.ImmutableCopy();

            Assert.False(copy is IMutableLookup<ILookup>);
        }

        [Fact]
        public void ImmutableCopy_Returns_EqualTo_Original()
        {
            string testKey = "Test Key: ImmutableCopy_Returns_EqualTo_Original";
            MutableLookup original = MutableLookupBuilder(testKey);
            ILookup copy = original.ImmutableCopy();

            Assert.True(original.Equals(copy));
        }

        [Fact]
        public void ImmutableCopy_Returns_Instance_With_EqualKey()
        {
            string testKey = "Test Key: MutableLookup.ImmutableCopy() returns an instance with equal (String.Equals) key to original";
            ILookup test = MutableLookupBuilder(testKey).ImmutableCopy();

            Assert.True(test.KeyCopy.Equals(testKey));
        }

        [Fact]
        public void ImmutableCopy_Returns_InstanceOf_Lookup()
        {
            string testKey = "Test Key: MutableLookup.ImmutableCopy() returns an instance of Lookup";
            ILookup test = MutableLookup.Build(testKey).ImmutableCopy();

            Assert.True(test is Lookup);
        }

        [Theory]
        [InlineData("Test Key: Equals_ImmutableCopy_Equals_Original")]
        [InlineData(null)]
        public void Equals_ImmutableCopy_Equals_this(string? key)
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
