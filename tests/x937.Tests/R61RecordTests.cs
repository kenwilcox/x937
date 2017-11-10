using Xunit;

namespace x937.Tests
{
    public class R61RecordTests
    {
        private readonly string _data;
        private readonly R61 _sut;

        public R61RecordTests()
        {
            var record = new Record("CreditDetailRecord", "61");
            _data = Builder.GetTestStringFor(record);
            _sut = new R61();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR61Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("61", _sut.RecordType);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_AuxiliaryOnUs()
        {
            Assert.Equal("xxxxxxxxxxxxxxx", _sut.AuxiliaryOnUs);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_ExternalProcessingCode()
        {
            Assert.Equal("-", _sut.ExternalProcessingCode);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_PayorBankRoutingNumber()
        {
            Assert.Equal("522000410", _sut.PayorBankRoutingNumber);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_CreditAccountNumberOnUs()
        {
            Assert.Equal("xxxxxxxxxxxxxxxxxxxx", _sut.CreditAccountNumberOnUs);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_ItemAccount()
        {
            Assert.Equal("9999999999", _sut.ItemAccount);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_ECEInstitutionItemNumber()
        {
            Assert.Equal("1 2 3 4 5 6 7 8", _sut.ECEInstitutionItemNumber);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_DocumentationTypeIndicator()
        {
            Assert.Equal("*", _sut.DocumentationTypeIndicator);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_TypeOfAccountCode()
        {
            Assert.Equal("-", _sut.TypeOfAccountCode);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_SourceOfWorkCode()
        {
            Assert.Equal("^", _sut.SourceOfWorkCode);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_WorkType()
        {
            Assert.Equal("-", _sut.WorkType);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_DebitCreditIndicator()
        {
            Assert.Equal("#", _sut.DebitCreditIndicator);
        }

        [Fact]
        public void TestThatR61_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("***", _sut.Reserved);
        }
    }
}
