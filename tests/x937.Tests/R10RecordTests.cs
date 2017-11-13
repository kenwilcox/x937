using Xunit;

namespace x937.Tests
{
    public class R10RecordTests
    {
        private readonly string _data;
        private readonly R10 _sut;

        public R10RecordTests()
        {
            var record = new Record("CashLetterHeaderRecord", "10");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R10();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR10Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("10", _sut.RecordType);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CollectionTypeIndicator()
        {
            Assert.Equal("01", _sut.CollectionTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_DestinationRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.DestinationRoutingNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_ECEInstutionRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ECEInstitutionRoutingNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterBusinessDate()
        {
            Assert.Equal("YYYYMMDD", _sut.CashLetterBusinessDate);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterCreationDate()
        {
            Assert.Equal("YYYYMMDD", _sut.CashLetterCreationDate);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterCreationTime()
        {
            Assert.Equal("HHmm", _sut.CashLetterCreationTime);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterRecordTypeIndicator()
        {
            Assert.Equal("I", _sut.CashLetterRecordTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterDocumentationTypeIndicator()
        {
            Assert.Equal("G", _sut.CashLetterDocumentationTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterId()
        {
            Assert.Equal("A1B2C3D4", _sut.CashLetterId);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_OriginatorContactName()
        {
            Assert.Equal("6Z-5Y-4X-3W-2V", _sut.OriginatorContactName);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_OriginatorContactPhoneNumber()
        {
            Assert.Equal("1234567890", _sut.OriginatorContactPhoneNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_FedWorkType()
        {
            Assert.Equal("-", _sut.FedWorkType);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_UserField()
        {
            Assert.Equal("--", _sut.UserField);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_User()
        {
            Assert.Equal("-", _sut.User);
        }
    }
}
