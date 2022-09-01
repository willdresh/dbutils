// LookupTests.cs
// Will Dresh
// w@dresh.app

using System;
using DBInterface;
using Xunit;

namespace DBInterface_XUnit_Tests
{
    public class LookupTests
    {
        [Fact]
        public void Build_KeyCopy_EqualTo_BuildParameter()
        {
            // Arrange
            string testKey = "Build Parameter Test Key";

            // Act
            Lookup test = Lookup.Build(testKey);

            // Assert
            Assert.Equal(test.KeyCopy, testKey);
        }

        [Fact]
        public void Equals_Instance_EqualTo_Same_Key()
        {
            // Arrange
            string testKey = "Equality Test Key";

            // Act
            Lookup test1 = Lookup.Build(testKey),
                test2 = Lookup.Build(testKey);

            // Assert
            Assert.True(test1.Equals(test2));
        }

        [Fact]
        public void ExclusivelyOwns_ReferenceTo_Key()
        {
            // Arrange
            string testKey_transform = "Transform Test Key",
                testKey_reference = "Reference Test Key";
            Lookup test_transform = Lookup.Build(testKey_transform),
                test_reference = Lookup.Build(testKey_reference);

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
