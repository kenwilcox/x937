using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class RecordTests
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
    }
}
