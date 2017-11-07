using Xunit;

namespace x937.Tests
{
    public class R10RecordTests
    {
        private const string R10Data = "100199999999999999999988888888888888884444IG88888888xxxxxxxxxxxxxxzzzzzzzzzz1221";

        [Fact]
        public void TestThatR10Data_IsTheRightLength()
        {
            Assert.Equal(80, R10Data.Length);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_RecordType()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("10", sut.RecordType);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CollectionTypeIndicator()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("01", sut.CollectionTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_DestinationRoutingNumber()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("999999999", sut.DestinationRoutingNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_ECEInstutionRoutingNumber()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("999999999", sut.ECEInstutionRoutingNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterBusinessDate()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("88888888", sut.CashLetterBusinessDate);;
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterCreationDate()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("88888888", sut.CashLetterCreationDate);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterCreationTime()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("4444", sut.CashLetterCreationTime);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterRecordTypeIndicator()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("I", sut.CashLetterRecordTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterDocumentationTypeIndicator()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("G", sut.CashLetterDocumentationTypeIndicator);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_CashLetterId()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("88888888", sut.CashLetterId);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_OriginatorContactName()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("xxxxxxxxxxxxxx", sut.OriginatorContactName);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_OriginatorContactPhoneNumber()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("zzzzzzzzzz", sut.OriginatorContactPhoneNumber);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_FedWorkType()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("1", sut.FedWorkType);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_UserField()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("22", sut.UserField);
        }

        [Fact]
        public void TestThatR10_HasTheRightValueFor_User()
        {
            var sut = new R10();
            sut.SetData(R10Data);
            Assert.Equal("1", sut.User);
        }
    }
}
