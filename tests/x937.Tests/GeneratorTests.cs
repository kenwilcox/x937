using x937.Meta;
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
        [InlineData("CheckDetailAddendumARecord", "26")]
        [InlineData("ImageViewDetailRecord", "50")]
        [InlineData("ImageViewDataRecord", "52")]
        [InlineData("CreditDetailRecord", "61")]
        [InlineData("BatchControlRecord", "70")]
        [InlineData("CashLetterControlRecord", "90")]
        [InlineData("FileControlRecord", "99")]
        public void TestThatGenerate_ReturnsExpectedCode(string recordName, string recordType)
        {
            // Arrange
            var key = new XRecord(recordName, recordType);
            var meta = Builder.GetMeta()[key];

            // Act
            var code = Builder.GetClassFor(key);

            // Assert
            foreach (var field in meta)
            {
                if (field.FieldName == "RecordType") continue; // base class, all have it

                // Set Data bits
                if (field.FieldType != FieldType.Binary)
                {
                    Assert.Contains($"{field.FieldName} = Data.Substring({field.Position.Start}, {field.Size})", code);
                }
                else
                {
                    //, Data.Length
                    Assert.Contains($"{field.FieldName} = optional;", code);
                }


                // Property bits
                var fieldType = "string";
                if (field.FieldType == FieldType.Binary) fieldType = "byte[]";
                Assert.Contains($"public {fieldType} {field.FieldName} {{ get; set; }}", code);
            }
        }
    }
}
