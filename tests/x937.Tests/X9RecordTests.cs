using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace x937.Tests
{
    public class X9RecordTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("     ")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void TestThat_SetDataTheSecondTime_IgnoresBeingSet(string data)
        {
            // Arrange
            var record = new TestX9();

            // Act
            record.SetData("TEST");
            record.SetData(data);

            // Assert
            Assert.Equal("TE", record.RecordType);
        }

        [Fact]
        public void TestThat_SetDefaultProperties_SetsStringValuesToNotNull()
        {
            // Arrange, Act
            var record = new TestX9();

            // Assert
            Assert.NotNull(record.NotNull);
        }

        [Fact]
        public void TestThat_X9RecordsReference_AreEqual()
        {
            // Arrange
            var record1 = new TestX9();
            var record2 = record1;

            // Act
            var result = record1.Equals(record2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TestThat_X9RecordAndNull_AreNotEqual()
        {
            // Arrange
            var record1 = new TestX9();

            // Act
            var result = record1.Equals(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TestThat_X9RecordsOfDifferentTypes_AreNotEqual()
        {
            // Arrange
            var record1 = new TestX9();
            var record2 = new TestOtherX9();

            // Act
            var result = record1.Equals(record2);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("TEST", "TEST", true)]
        [InlineData("TEST", null, false)]
        [InlineData(null, "TEST", false)]
        [InlineData(null, null, true)]
        public void TestThat_X9RecordsWithTheSamePropertyValues_AreEqual(string val1, string val2, bool expected)
        {
            // Arrange
            var record1 = new TestOtherX9 {NotNull = val1};
            var record2 = new TestOtherX9 {NotNull = val2};

            // Assert
            var result = record1.Equals(record2);

            // Assert
            Assert.Equal(expected, result);
        }

        public class TestX9 : X9Record
        {
            public string NotNull { get; set; }
        }

        public class TestOtherX9 : X9Record
        {
            public string NotNull { get; set; }
        }

    }
}
