using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class XRecordTests
    {
        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("FieldHeader", "01", "FieldHeader", "01")]
        [InlineData("CashHeader", "10", "CashHeader", "10")]
        public void TestThat_SameRecords_AreEqual(string name1, string typeId1, string name2, string typeId2)
        {
            // Arrange
            var record1 = new XRecord(name1, typeId1);
            var record2 = new XRecord(name2, typeId2);

            // Act
            var result = record1.Equals(record2);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("FieldHeader", "01", "FieldHeader", "01")]
        [InlineData("CashHeader", "10", "CashHeader", "10")]
        public void TestThat_SameRecordsAsObject_AreEqual(string name1, string typeId1, string name2, string typeId2)
        {
            // Arrange
            var record1 = new XRecord(name1, typeId1);
            var record2 = new XRecord(name2, typeId2);

            // Act
            var result = record1.Equals((object)record2);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("FieldHeader", "01", "CashHeader", "10")]
        [InlineData("CashHeader", "10", "FieldHeader", "01")]
        public void TestThat_DifferentRecords_AreNotEqual(string name1, string typeId1, string name2, string typeId2)
        {
            // Arrange
            var record1 = new XRecord(name1, typeId1);
            var record2 = new XRecord(name2, typeId2);

            // Act
            var result = record1 != record2;

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("FieldHeader", "01")]
        [InlineData("CashHeader", "10")]
        public void TestThat_ARecordWithNull_AreNotEqual(string name, string typeId)
        {
            // Arrange
            var record = new XRecord(name, typeId);

            // Act
            var result = record == null || null == record;

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("FieldHeader", "01")]
        [InlineData("CashHeader", "10")]
        public void TestThat_ExactSameRecords_AreEqual(string name, string typeId)
        {
            // Arrange
            var record1 = new XRecord(name, typeId);
            var record2 = record1;

            // Act
            var result = record1 == record2;

            // Assert
            Assert.True(result);
        }
    }
}
