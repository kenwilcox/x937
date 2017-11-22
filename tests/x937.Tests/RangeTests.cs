using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class RangeTests
    {
        [Theory]
        [InlineData(1, 2, 1, 2)]
        [InlineData(65535, 0, 65535, 0)]
        public void TestThat_SameRanges_AreEqual(int start1, int end1, int start2, int end2)
        {
            // Arrange
            var range1 = new Range(start1, end1);
            var range2 = new Range(start2, end2);

            // Act
            var result = range1 == range2;
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1, 2, 2, 1)]
        [InlineData(0, 65535, 65535, 0)]
        public void TestThat_DifferentRanges_AreNotEqual(int start1, int end1, int start2, int end2)
        {
            // Arrange
            var range1 = new Range(start1, end1);
            var range2 = new Range(start2, end2);

            // Act
            var result = range1 != range2;

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1, 2, 1, 2)]
        [InlineData(65535, 0, 65535, 0)]
        public void TestThat_SameRangesAsObjects_AreEqual(int start1, int end1, int start2, int end2)
        {
            // Arrange
            var range1 = new Range(start1, end1);
            var range2 = new Range(start2, end2);

            // Act
            var result = range1.Equals((object)range2);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(101, 42)]//, 265406940)]
        [InlineData(-1, -1)]//, 802282331)]
        [InlineData(65535, 65535)]//, -2063869093)]
        [InlineData(-65535, 65535)]//, 626296237)]
        public void TestThat_GetHashCode_ReturnsValidHash(int start, int end)
        {
            // Arrange
            var range = new Range(start, end);

            // Act
            var hash = range.GetHashCode();
            var expected = new {Start = start, End = end}.GetHashCode();

            // Assert
            Assert.Equal(expected, hash);
        }
    }
}
