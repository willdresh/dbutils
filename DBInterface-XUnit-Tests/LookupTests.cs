// LookupTests.cs
// Will Dresh
// w@dresh.app

using System;
using DBInterface;
//using Xunit; // TODO- need a reference

namespace DBInterface_XUnit_Tests
{
    internal class LookupTests
    {
        private Lookup lu;

        // TODO- need a reference
        //[Fact]
        public void Lookup_ExclusivelyOwns_ReferenceTo_Key()
        {
            // Arrange
            string testKey_transform = "Transform Test Key";
            Lookup test_transform = Lookup.Build(testKey_transform);
            string testKey_reference = "Reference Test Key";
            Lookup test_reference = Lookup.Build(testKey_reference);

            // Act
            testKey_transform += " - Transformation Applied";

            // Assert
            //Assert.False(ReferenceEquals(testKey_reference, test_reference.KeyCopy));
            //Assert.False(testKey_transform.Equals(test_transform.KeyCopy));
        }

        private void NewLookupInstanceNullKey()
        {
            lu = Lookup.Build(null);
        }

        private void NewLookupInstance(string newKey)
        {
            lu = Lookup.Build(newKey);
        }

    }
}
