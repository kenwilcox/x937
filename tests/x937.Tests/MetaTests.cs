using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace x937.Tests
{
    public class MetaTests
    {
        private readonly Meta _metadata;
        public MetaTests()
        {
            _metadata = Builder.GetMeta();
        }

        [Theory]
        [InlineData("FileHeaderRecord", "01", 80)]
        [InlineData("CashLetterHeaderRecord", "10", 80)]
        [InlineData("BatchHeaderRecord", "20", 80)]
        [InlineData("CheckDetailRecord", "25", 80)]
        public void TestMeta_ObjectFieldLength(string recordName, string recordType, int expectedLength)
        {
            // Arrange
            var key = new Record(recordName, recordType);
            var meta = _metadata[key];
            var record = Builder.GetObjectFor(key);

            // Act
            var size = meta.Sum(x => x.Size);
            var props = record.GetType().GetProperties().Select(x => x.Name).ToList();
            var mprops = meta.Select(x => x.FieldName).ToList();
            mprops.Sort();
            props.Sort();

            // Assert
            Assert.Equal(expectedLength, size);
            // This is great if you don't care to know what field is missing or wrong
            //Assert.Equal(mprops, props);
            foreach (var field in mprops)
            {
                Assert.Contains(field, props);
            }
        }

        [Theory]
        [InlineData("FileHeaderRecord", "01", 80)]
        [InlineData("CashLetterHeaderRecord", "10", 80)]
        [InlineData("BatchHeaderRecord", "20", 80)]
        [InlineData("CheckDetailRecord", "25", 80)]
        public void TestMeta_MetaFieldCount(string recordName, string recordType, int expectedCount)
        {
            // Arrange
            var meta = _metadata[new Record(recordName, recordType)];

            // Act
            var list = new List<int>();
            foreach (var field in meta)
            {
                var start = field.Position.Start;
                var end = field.Position.End;
                for (var i = start; i < end; i++)
                    list.Add(i);
            }
            var mlist = new List<int>();
            for (var i = 0; i < expectedCount; i++)
                mlist.Add(i);


            // Assert
            Assert.Equal(expectedCount, list.Count);
            Assert.Equal(mlist, list);
        }

        [Theory]
        [InlineData("FileHeaderRecord", "01", 14)]
        [InlineData("CashLetterHeaderRecord", "10", 15)]
        [InlineData("BatchHeaderRecord", "20", 12)]
        [InlineData("CheckDetailRecord", "25", 15)]
        public void TestMeta_MetaFieldCounts(string recordName, string recordType, int fieldCount)
        {
            // Arrange, Act
            var meta = _metadata[new Record(recordName, recordType)];
            // Assert
            Assert.Equal(fieldCount, meta.Count);
        }

        [Theory]
        [InlineData("FileHeaderRecord", "01", 14)]
        [InlineData("CashLetterHeaderRecord", "10", 15)]
        [InlineData("BatchHeaderRecord", "20", 12)]
        [InlineData("CheckDetailRecord", "25", 15)]
        public void TestMeta_MetaFieldNumbers(string recordName, string recordType, int fieldCount)
        {
            // Arrange
            var meta = _metadata[new Record(recordName, recordType)];

            // Act, Assert
            var counter = 0;
            for (var i = 1; i <= fieldCount; i++)
            {
                counter++;
                var field = meta.Where(x => x.Order == i).ToList();
                Assert.NotNull(field);
                Assert.Single(field);
            }
            Assert.Equal(fieldCount, counter);
        }

        [Fact]
        public void TestMeta_ExpectedStringValuesForR01()
        {
            // Arrange
            var key = new Record("FileHeaderRecord", "01");
            const int length = 80;
            var record = Builder.GetObjectFor(key);

            // Act
            var str = Builder.GetTestStringFor(key);
            record.SetData(str);
            var rec = record as R01;

            // Assert
            Assert.NotNull(rec);
            Assert.Equal(length, str.Length);
            Assert.Equal("01", rec.RecordType);
            Assert.Equal("03", rec.StandardLevel);
            Assert.Matches(@"^[T|P]$", rec.TestFileIndicator);
            Assert.Equal("TTTTAAAAC", rec.ImmediateDestinationRoutingNumber);
            Assert.Equal("TTTTAAAAC", rec.ImmediateOriginRoutingNumber);
            Assert.Equal("YYYYMMDD", rec.FileCreationDate);
            Assert.Equal("HHmm", rec.FileCreationTime);
            Assert.Equal("N", rec.ResendIndicator);
            Assert.Equal("ABCDEFGHIJKLMNOPQR", rec.ImmediateDestinationName);
            Assert.Equal("ABCDEFGHIJKLMNOPQR", rec.ImmediateOriginName);
            Assert.Equal("A", rec.FileIdModifier);
            Assert.Equal("  ", rec.CountryCode);
            Assert.Equal("CR61", rec.UserField);
            Assert.Equal(" ", rec.Reserved);
        }

        [Fact]
        public void TestMeta_ExpectedStringValuesForR10()
        {
            // Arrange
            var key = new Record("CashLetterHeaderRecord", "10");
            const int length = 80;
            var record = Builder.GetObjectFor(key);

            // Act
            var str = Builder.GetTestStringFor(key);
            record.SetData(str);
            var rec = record as R10;

            // Assert
            Assert.NotNull(rec);
            Assert.Equal(length, str.Length);
            Assert.Equal("10", rec.RecordType);
            Assert.Equal("01", rec.CollectionTypeIndicator);
            Assert.Equal("TTTTAAAAC", rec.DestinationRoutingNumber);
            Assert.Equal("TTTTAAAAC", rec.ECEInstitutionRoutingNumber);
            Assert.Equal("YYYYMMDD", rec.CashLetterBusinessDate);
            Assert.Equal("YYYYMMDD", rec.CashLetterCreationDate);
            Assert.Equal("HHmm", rec.CashLetterCreationTime);
            Assert.Equal("I", rec.CashLetterRecordTypeIndicator);
            Assert.Equal("G", rec.CashLetterDocumentationTypeIndicator);
            Assert.Equal("A1B2C3D4", rec.CashLetterId);
            Assert.Equal("6Z-5Y-4X-3W-2V", rec.OriginatorContactName);
            Assert.Equal("1234567890", rec.OriginatorContactPhoneNumber);
            Assert.Equal("  ", rec.UserField);
            Assert.Equal(" ", rec.User);
        }

        [Fact]
        public void TestMeta_ExpectedStringValuesForR20()
        {
            // Arrange
            var key = new Record("BatchHeaderRecord", "20");
            const int length = 80;
            var record = Builder.GetObjectFor(key);

            // Act
            var str = Builder.GetTestStringFor(key);
            record.SetData(str);
            var rec = record as R20;

            // Assert
            Assert.NotNull(rec);
            Assert.Equal(length, str.Length);
            Assert.Equal("20", rec.RecordType);
            Assert.Equal("01", rec.CollectionTypeIndicator);
            Assert.Equal("TTTTAAAAC", rec.DestinationRoutingNumber);
            Assert.Equal("TTTTAAAAC", rec.ECEInstitutionRoutingNumber);
            Assert.Equal("YYYYMMDD", rec.BatchBusinessDate);
            Assert.Equal("YYYYMMDD", rec.BatchCreationDate);
            Assert.Equal("          ", rec.BatchId);
            Assert.Equal("4242", rec.BatchSequenceNumber);
            Assert.Equal("  ", rec.CycleNumber);
            Assert.Equal("TTTTAAAAC", rec.ReturnLocationRoutingNumber);
            Assert.Equal("     ", rec.UserField);
            Assert.Equal("            ", rec.Reserved);
        }
    }
}
