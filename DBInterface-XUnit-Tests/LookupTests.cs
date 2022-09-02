// LookupTests.cs
// Will Dresh
// w@dresh.app

using System;
using DBInterface_XUnit_Tests.ExternalTypes;
using DBInterface;
using Xunit;
using System.Reflection.Metadata.Ecma335;

namespace DBInterface_XUnit_Tests
{

    public class LookupTests
    {
        private static readonly Func<string?, MutableLookup> MutableLookupBuilder = MutableLookup.Build;
        private static readonly Func<string?, Lookup> LookupBuilder = Lookup.Build;

        [Fact]
        public void Equals_Lookup_With_NullKey_Equals_MutableLookup_With_NullKey()
        {
            Assert.True(LookupBuilder(null).Equals(MutableLookupBuilder(null)));
        }

        [Fact]
        public void Equals_TwoLookups_With_NullKeys_Are_Equal()
        {
            Assert.True(LookupBuilder(null).Equals(LookupBuilder(null)));
        }

        [Fact]
        public void Build_AllowsNullKey()
        {
            Assert.True(LookupBuilder(null) is Lookup);
        }

        [Fact]
        public void Build_KeyCopy_ValueEqualTo_BuildParameter()
        {
            // Arrange
            string testKey = "Lookup.Build - KeyCopy is value-equal to build parameter - test key";
            Lookup test;

            // Act
            test = LookupBuilder(testKey);

            // Assert
            Assert.True(testKey.Equals(test.KeyCopy));
        }

        [Fact]
        public void Equals_Instance_EqualTo_MutableLookup_Same_Key()
        {
            // Arrange
            string testKey = "Lookup.Equals - equality with MutableLookup - Test Key";
            Lookup testImmutable;
            MutableLookup testMutable;

            // Act
            testImmutable = LookupBuilder(testKey);
            testMutable = MutableLookupBuilder(testKey);

            // Assert
            Assert.True(testImmutable.Equals(testMutable));
        }

        [Fact]
        public void Build_KeyCopy_EqualTo_BuildParameter()
        {
            // Arrange
            string testKey = "Build Parameter Test Key";

            // Act
            Lookup test = LookupBuilder(testKey);

            // Assert
            Assert.Equal(test.KeyCopy, testKey);
        }

        [Fact]
        public void Equals_Instance_EqualTo_Same_Key()
        {
            // Arrange
            string testKey = "Equality Test Key";

            // Act
            Lookup test1 = LookupBuilder(testKey),
                test2 = LookupBuilder(testKey);

            // Assert
            Assert.True(test1.Equals(test2));
        }

        [Fact]
        public void ExclusivelyOwns_ReferenceTo_Key()
        {
            // Arrange
            string testKey_transform = "Transform Test Key",
                testKey_reference = "Reference Test Key";
            Lookup test_transform = LookupBuilder(testKey_transform),
                test_reference = LookupBuilder(testKey_reference);

            // Act
            testKey_transform += " - Transformation Applied";
            bool keysAreReferenceEqual = ReferenceEquals(testKey_reference, test_reference.KeyCopy);
            bool ableToTransformKeyExternally = testKey_transform.Equals(test_transform.KeyCopy);

            // Assert
            Assert.False(keysAreReferenceEqual);
            Assert.False(ableToTransformKeyExternally);
        }

    }
}
