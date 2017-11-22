using System;
using System.Collections.Generic;
using x937.Meta;
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
            var record = new XRecord(name, typeId);

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
                new Field {FieldType = Meta.FieldType.Binary},
            };
            // Act
            var exception = Record.Exception(() => Builder.GetTestStringFor(fields));

            // Assert
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Theory]
        [InlineData("FileHeaderRecord", "01")]
        [InlineData("CashLetterHeaderRecord", "10")]
        [InlineData("BuilderIgnoresTheName", "01")]
        [InlineData("CashLetterHeaderRecord", "98")]
        public void TestThatRecords_ReturnsExactObject(string name, string typeId)
        {
            // Arrange
            var record = new XRecord(name, typeId);

            // Act
            var obj = Builder.GetObjectFor(record);
            var sut = Builder.GetObjectFor(record);

            // Assert
            var ret = sut.Equals(obj);
            Assert.True(ret);
        }

        [Fact]
        public void TestThat_GetTestStringFor_CoversUndefinedEdgeCase()
        {
            // Arrange
            var fields = new List<Field>
            {
                new Field { Type = "NotFound" },
            };

            // Act
            var str = Builder.GetTestStringFor(fields);

            // Assert
            Assert.Equal("-", str);
        }

        [Fact]
        public void TestThat_GetTestStringFor_HandlesBinaryException()
        {
            // Arrange
            var fields = new List<Field>
            {
                new Field { FieldType = Meta.FieldType.Binary },
                new Field { FieldType = Meta.FieldType.Length},
            };

            // Act
            var str = "";
            var exception = Record.Exception(() => str = Builder.GetTestStringFor(fields));

            // Assert
            Assert.Equal("", str);
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public void TestThat_GetTestStringFor_HandlesBinarySizeException()
        {
            // Arrange
            var fields = new List<Field>
            {
                new Field { FieldType = Meta.FieldType.Binary },
                new Field { FieldType = Meta.FieldType.Length},
            };

            // Act
            var str = "";
            var exception = Record.Exception(() => str = Builder.GetTestStringFor(fields));

            // Assert
            Assert.Equal("", str);
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public void TestThat_GetTestStringFor_ThrowsWhenSizeIsInvalid()
        {
            // Arrange
            var fields = new List<Field>
            {
                new Field{DocPosition = new Range(1, -2)}
            };

            // Act
            var str = "";
            var exception = Record.Exception(() => str = Builder.GetTestStringFor(fields));

            // Assert
            Assert.Equal("", str);
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }
    }
}
