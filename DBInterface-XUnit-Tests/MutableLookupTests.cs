using DBInterface;
using System;

namespace DBInterface_XUnit_Tests
{
    public class MutableLookupTests
    {
        internal static readonly Func<string, MutableLookup> MutableLookupBuilder = MutableLookup.Build;
        internal static readonly Func<string, Lookup> LookupBuilder = Lookup.Build;

        public class ImmutableCopy_Behavior
        {
            /// <summary>
            /// Assert that ImmutableCopy() provides an instance with an
            /// equal (according to String.Equals) key to that of the instance
            /// upon which ImmutableCopy() was called.
            /// </summary>
            [Fact]
            public void ImmutableCopy_KeyCopy_StringEquals_this_KeyCopy()
            {
                string testKey = "test this.ImmutableCopy().KeyCopy";
                MutableLookup test1 = MutableLookupBuilder(testKey);

                Assert.Equal(testKey, test1.ImmutableCopy().KeyCopy);
            }

            // If this fails, it means that IMutableLookup<ILookup> was not implemented
            // as intended by design.
            // Design intention is that IMutableLookup<ILookup>.ImmutableCopy() returns an instance
            // which implements ILookup AND does NOT implement IMutableLookup<ILookup>.
            [Fact]
            public void ImmutableCopy_Returns_Not_InstanceOf_IMutableLookup_of_ILookup()
            {
                string testKey = "test";
                MutableLookup original = MutableLookupBuilder(testKey);
                ILookup copy = original.ImmutableCopy();

                Assert.False(copy is IMutableLookup<ILookup>);
            }

            [Fact]
            public void ImmutableCopy_Returns_MutableLookup_DOT_Equals_Original()
            {
                string testKey = "Test Key: ImmutableCopy_Returns_EqualTo_Original";
                MutableLookup original = MutableLookupBuilder(testKey);
                ILookup copy = original.ImmutableCopy();

                Assert.True(original.Equals(copy));
            }

            // This test removed because it will fail when using externally-defined types.
            // the test should succeed for internal types, though.
            /*
            [Fact]
            public void ImmutableCopy_Returns_ILookup_Equals_Original()
            {
                string testKey = "foo";
                MutableLookup original = MutableLookupBuilder(testKey);
                ILookup copy = original.ImmutableCopy();

                Assert.True(copy.Equals(original));
            }
            */

            [Fact]
            public void ImmutableCopy_Returns_Instance_With_EqualKey()
            {
                string testKey = "Test Key: MutableLookup.ImmutableCopy() returns an instance with equal (String.Equals) key to original";
                ILookup test = MutableLookupBuilder(testKey).ImmutableCopy();

                Assert.True(test.KeyCopy.Equals(testKey));
            }
        }

        public class Equality {

            [Theory]
            [InlineData("test value-equality with lookup")]
            [InlineData(null)]
            public void EqualTo_Immutable_with_Same_Key(string testKey)
            {
                MutableLookup test1 = MutableLookupBuilder(testKey);
                Lookup test2 = LookupBuilder(testKey);

                Assert.True(test1.Equals(test2));
            }

            [Fact]
            public void Implements_ValueEquality()
            {
                string testKey = "test Equals_Implements_ValueEquality_With_MutableLookup";
                MutableLookup test1 = MutableLookupBuilder(testKey),
                    test2 = MutableLookupBuilder(testKey);

                Assert.False(ReferenceEquals(test1.KeyCopy, test2.KeyCopy));
                Assert.True(test1.Equals(test2));
                Assert.True(test2.Equals(test1));
            }

            [Theory]
            [InlineData("Test Key: Equals_ImmutableCopy_Equals_Original")]
            [InlineData(null)]
            public void ImmutableCopy_EqualTo_this(string key)
            {
                MutableLookup test = MutableLookupBuilder(key);

                Assert.True(test.Equals(test.ImmutableCopy()));
            }

            [Theory]
            [InlineData("Self-Equality Test Key")]
            [InlineData(null)]
            public void EqualTo_Self(string key)
            {
                MutableLookup test = MutableLookupBuilder(key);

                Assert.True(test.Equals(test));
            }
        }

        public class Builder
        {
            [Theory]
            [InlineData("test")]
            [InlineData(null)]
            public void BuildCopy_This_Equals_Copy(string key)
            {
                MutableLookup test1 = MutableLookupBuilder(key);
                MutableLookup test2 = MutableLookup.BuildCopy(test1);

                Assert.True(test1.Equals(test2));
                Assert.True(test2.Equals(test1));
            }

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

            [Fact]
            public void Build_KeyCopy_ValueEqualTo_Build_Parameter()
            {
                string testKey = "Test Key: MutableLookup.Build returns an instance whose KeyCopy is value-equal (String.Equals) to the parameter";
                MutableLookup test = MutableLookupBuilder(testKey);

                Assert.True(testKey.Equals(test.KeyCopy));
            }

            [Fact]
            public void Build_KeyCopy_NotReferenceEqualTo_Build_Parameter()
            {
                string testKey = "Test Key: MutableLookup.Build returns an instance whose KeyCopy is NOT reference-equal to the parameter";
                MutableLookup test = MutableLookupBuilder(testKey);

                Assert.False(ReferenceEquals(testKey, test.KeyCopy));
            }

            [Fact]
            public void Build_ReadOnlyKey_ReferenceEqualTo_Build_Parameter()
            {
                string testKey = "Test Key: MutableLookup.Build returns an instance whose ReadOnlyKey is reference-equal to the parameter";
                MutableLookup test = MutableLookupBuilder(testKey);

                Assert.True(ReferenceEquals(testKey, test.GetReadOnlyKey()));
            }
        }
    }
}
