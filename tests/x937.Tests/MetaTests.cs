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
        [InlineData("CheckDetailAddendumARecord", "26", 80)]
        [InlineData("ImageViewDetailRecord", "50", 80)]
        [InlineData("ImageViewDataRecord", "52", 117)]
        [InlineData("CreditDetailRecord", "61", 80)]
        [InlineData("BatchControlRecord", "70", 80)]
        public void TestMeta_ObjectFieldLength(string recordName, string recordType, int expectedLength)
        {
            // Arrange
            var key = new Record(recordName, recordType);
            var meta = _metadata[key];
            var record = Builder.GetObjectFor(key);

            // Act
            var size = meta.Where(x =>x.ValueType != ValueType.Binary).Sum(x => x.Size);
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
        [InlineData("CheckDetailAddendumARecord", "26", 80)]
        [InlineData("ImageViewDetailRecord", "50", 80)]
        [InlineData("ImageViewDataRecord", "52", 117)]
        [InlineData("CreditDetailRecord", "61", 80)]
        [InlineData("BatchControlRecord", "70", 80)]
        public void TestMeta_MetaFieldCount(string recordName, string recordType, int expectedCount)
        {
            // Arrange
            var meta = _metadata[new Record(recordName, recordType)];

            // Act
            var list = new List<int>();
            foreach (var field in meta)
            {
                if (field.ValueType == ValueType.Binary) continue;
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
        [InlineData("CheckDetailAddendumARecord", "26", 13)]
        [InlineData("ImageViewDetailRecord", "50", 17)]
        [InlineData("ImageViewDataRecord", "52", 17)]
        [InlineData("CreditDetailRecord", "61", 13)]
        [InlineData("BatchControlRecord", "70", 7)]
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
        [InlineData("CheckDetailAddendumARecord", "26", 13)]
        [InlineData("ImageViewDetailRecord", "50", 17)]
        [InlineData("ImageViewDataRecord", "52", 17)]
        [InlineData("CreditDetailRecord", "61", 13)]
        [InlineData("BatchControlRecord", "70", 7)]
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
        public void TestMeta_ContainsValidUsage(string recordName, string recordType)
        {
            // Arrange
            var meta = _metadata[new Record(recordName, recordType)];

            // Act, Assert
            foreach (var field in meta)
            {
                Assert.Matches(@"^[M|C|O]$", field.Usage);
            }
        }
    }
}
