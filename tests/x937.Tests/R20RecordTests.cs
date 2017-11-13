using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class R20RecordTests
    {
        private readonly string _data;
        private readonly R20 _sut;

        public R20RecordTests()
        {
            var record = new XRecord("BatchHeaderRecord", "20");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R20();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR20Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("20", _sut.RecordType);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_CollectionTypeIndicator()
        {
            Assert.Equal("01", _sut.CollectionTypeIndicator);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_DestinationRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.DestinationRoutingNumber);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_ECEInstutionRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ECEInstitutionRoutingNumber);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_BatchBusinessDate()
        {
            Assert.Equal("YYYYMMDD", _sut.BatchBusinessDate);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_BatchCreationDate()
        {
            Assert.Equal("YYYYMMDD", _sut.BatchCreationDate);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_BatchId()
        {
            Assert.Equal("----------", _sut.BatchId);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_BatchSequenceNumber()
        {
            Assert.Equal("4242", _sut.BatchSequenceNumber);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_CycleNumber()
        {
            Assert.Equal("--", _sut.CycleNumber);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_ReturnLocationRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ReturnLocationRoutingNumber);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_UserField()
        {
            Assert.Equal("-----", _sut.UserField);
        }

        [Fact]
        public void TestThatR20_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("------------", _sut.Reserved);
        }
    }
}
