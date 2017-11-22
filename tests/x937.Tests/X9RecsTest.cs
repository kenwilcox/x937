using Xunit;

namespace x937.Tests
{
    public class X9RecsTest
    {
        [Fact]
        public void TestThat_AddingASingleItem_ContainsASingleItem()
        {
            // Arrange, Act
            var rec = new X9Rec();
            var recs = new X9Recs {rec};

            // Assert
            Assert.Single(recs);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-2, 2)]
        public void TestThat_RemovingAnItem_RemovesThatItem(int index, int expected)
        {
            // Arrange
            var rec = new X9Rec();
            var recs = new X9Recs {rec, rec};

            // Act
            recs.Remove(index);
            var count = recs.Count;

            // Assert
            Assert.Equal(expected, count);
        }

    }
}
