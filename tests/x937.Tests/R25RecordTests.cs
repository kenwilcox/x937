using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class R25RecordTests
    {
        private readonly string _data;
        private readonly R25 _sut;

        public R25RecordTests()
        {
            var record = new XRecord("CheckDetailRecord", "25");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R25();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR25Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("25", _sut.RecordType);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_AuxiliaryOnUs()
        {
            Assert.Equal("xxxxxxxxxxxxxxx", _sut.AuxiliaryOnUs);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_ExternalProcessingCode()
        {
            Assert.Equal("x", _sut.ExternalProcessingCode);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_PayorBankRoutingNumber()
        {
            Assert.Equal("TTTTAAAA", _sut.PayorBankRoutingNumber);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_PriorBankRoutingNumberCheckDigit()
        {
            Assert.Equal("C", _sut.PriorBankRoutingNumberCheckDigit);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_OnUs()
        {
            Assert.Equal("zzzzzzzzzzzzzzzzzzzz", _sut.OnUs);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_ItemAmount()
        {
            Assert.Equal("1234567890", _sut.ItemAmount);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_ECEInstitutionItemSequenceNumber()
        {
            Assert.Equal("1 2 3 4 5 6 7 8", _sut.ECEInstitutionItemSequenceNumber);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_DocumentationTypeIndicator()
        {
            Assert.Equal("G", _sut.DocumentationTypeIndicator);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_ReturnAcceptanceIndicator()
        {
            Assert.Equal("6", _sut.ReturnAcceptanceIndicator);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_MICRValidIndicator()
        {
            Assert.Matches(@"^[1|2|3|4]$", _sut.MICRValidIndicator);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_BOFDIndicator()
        {
            Assert.Matches(@"^[Y|N|U]$", _sut.BOFDIndicator);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_CheckDetailRecordAddendumCount()
        {
            Assert.Equal("12", _sut.CheckDetailRecordAddendumCount);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_CorrectionIndicator()
        {
            Assert.Matches(@"^[0|1|2|3|4]$", _sut.CorrectionIndicator);
        }

        [Fact]
        public void TestThatR25_HasTheRightValueFor_ArchiveTypeIndicator()
        {
            Assert.Equal("-", _sut.ArchiveTypeIndicator);
        }
    }
}
