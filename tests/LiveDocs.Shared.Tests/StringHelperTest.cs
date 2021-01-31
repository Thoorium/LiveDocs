using System;
using Xunit;

namespace LiveDocs.Shared.Tests
{
    public class StringHelperTest
    {
        [Theory]
        [InlineData("hello", "helloo", 1)]
        [InlineData("Cat", "Dog", 3)]
        [InlineData("Cat", "Cta", 1)]
        [InlineData("Cat", "cAT", 3)]
        [InlineData("Cat", "Ca", 1)]
        [InlineData("Ca", "cat", 2)]
        public void DamerauLevenshteinDistance(string source, string target, int expectedDistance)
        {
            int distance = StringHelper.DamerauLevenshteinDistance(source, target);
            Assert.Equal(expectedDistance, distance);
        }
    }
}