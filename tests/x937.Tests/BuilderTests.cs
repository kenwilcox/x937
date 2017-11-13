using System;
using System.Collections.Generic;
using Xunit;

namespace x937.Tests
{
    public class BuilderTests
    {
        [Theory]
        [InlineData("", "", true)]
        [InlineData("FileHeaderRecord", "01", false)]
        [InlineData("CashLetterHeaderRecord", "10", false)]
        [InlineData("BuilderIgnoresTheName", "01", false)]
        [InlineData("CashLetterHeaderRecord", "98", true)]
        public void TestThatRecords_ReturnsExpectedObject(string name, string typeId, bool isUnknown)
        {
            // Arrange
            var record = new Record(name, typeId);

            // Act
            var obj = Builder.GetObjectFor(record);

            // Assert
            if (isUnknown) Assert.True(obj is Unknown);
            else Assert.True(!(obj is Unknown));
        }

        [Fact]
        public void TestThat_BinaryBeforeLengthFieldsFail()
        {
            // Arrange
            var fields = new List<Field>
            {
                new Field {ValueType = ValueType.Binary},
            };
            // Act
            var exception = Xunit.Record.Exception(() => Builder.GetTestStringFor(fields));

            // Assert
            Assert.IsType<InvalidOperationException>(exception);
        }
    }
}
