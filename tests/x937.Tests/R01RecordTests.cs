using Xunit;

namespace x937.Tests
{
    public class R01RecordTests
    {
        private readonly string _data;
        private readonly R01 _sut;

        public R01RecordTests()
        {
            var record = new Record("FileHeaderRecord", "01");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R01();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR01Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("01", _sut.RecordType);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_StandardLevel()
        {
            Assert.Equal("03", _sut.StandardLevel);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_TestFileIndicator()
        {
            Assert.Matches(@"^[T|P]$", _sut.TestFileIndicator);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_ImmediateDestinationRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ImmediateDestinationRoutingNumber);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_ImmediateOriginRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ImmediateOriginRoutingNumber);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_FileCreationDate()
        {
            Assert.Equal("YYYYMMDD", _sut.FileCreationDate);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_FileCreationTime()
        {
            Assert.Equal("HHmm", _sut.FileCreationTime);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_ResendIndicator()
        {
            Assert.Equal("N", _sut.ResendIndicator);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_ImmediateDestinationName()
        {
            Assert.Equal("ABCDEFGHIJKLMNOPQR", _sut.ImmediateDestinationName);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_ImmediateOriginName()
        {
            Assert.Equal("ABCDEFGHIJKLMNOPQR", _sut.ImmediateOriginName);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_FileIdModifier()
        {
            Assert.Equal("A", _sut.FileIdModifier);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_CountryCode()
        {
            Assert.Equal("--", _sut.CountryCode);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_UserField()
        {
            Assert.Equal("CR61", _sut.UserField);
        }

        [Fact]
        public void TestThatR01_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("-", _sut.Reserved);
        }
    }
}
