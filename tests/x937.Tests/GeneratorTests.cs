using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace x937.Tests
{
    public class GeneratorTests
    {
        [Theory]
        [InlineData("FileHeaderRecord", "01")]
        [InlineData("CashLetterHeaderRecord", "10")]
        [InlineData("BatchHeaderRecord", "20")]
        [InlineData("CheckDetailRecord", "25")]
        public void TestThatGenerate_ReturnsExpectedCode(string recordName, string recordType)
        {
            // Arrange
            var key = new Record(recordName, recordType);
            var meta = Builder.GetMeta()[key];

            // Act
            var code = Builder.GetClassFor(key);

            // Assert
            foreach (var field in meta)
            {
                if (field.FieldName == "RecordType") continue; // base class, all have it

                // Set Data bits
                Assert.Contains($"{field.FieldName} = Data.Substring({field.Position.Start}, {field.Size})", code);

                // Property bits
                Assert.Contains($"public string {field.FieldName} {{ get; set; }}", code);
            }
        }
    }
}
