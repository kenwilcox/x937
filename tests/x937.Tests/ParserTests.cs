using System;
using System.IO;
using Xunit;

namespace x937.Tests
{
    [Collection("ICL Files")]
    public class ParserTests
    {
        private readonly TestUtilsFixture _fixture;

        public ParserTests(TestUtilsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestThat_ParsingACorrectFile_Works()
        {
            // Arrange
            var file = _fixture.ValidICLFile;

            // Act
            var recs = Parser.ParseX9File(file);

            // Assert
            Assert.Equal(21, recs.Count);
        }

        // TODO Generate additional test files that are broken...
    }
}
