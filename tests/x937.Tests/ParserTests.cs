using System;
using System.IO;
using Xunit;

namespace x937.Tests
{
    public class ParserTests
    {
        [Fact]
        public void TestThat_ParsingACorrectFile_Works()
        {
            // Arrange
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218.ICL");

            // Act
            var recs = Parser.ParseX9File(file);

            // Assert
            Assert.Equal(21, recs.Count);
        }

        // TODO Generate additional test files that are broken...
    }
}
