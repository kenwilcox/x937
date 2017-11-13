using Xunit;

namespace x937.Tests
{
    public class SummaryEmptyTests
    {
        private readonly Summary _sut;

        public SummaryEmptyTests()
        {
            _sut = Summary.GetSummary(new X9Recs());
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasASummary()
        {
            Assert.NotNull(_sut);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasAnEmptyCashLetterSummary()
        {
            Assert.Empty(_sut.CashLetterSummary);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasAnEmptyCreditSummary()
        {
            Assert.Empty(_sut.CreditSummary);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasANotNullBankName()
        {
            Assert.NotNull(_sut.FileDetail.BankName);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasANotNullFileCreationDate()
        {
            Assert.NotNull(_sut.FileDetail.FileCreationDate);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasANotNullFileCreationTime()
        {
            Assert.NotNull(_sut.FileDetail.FileCreationTime);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_TotalNumberofRecordsIsZero()
        {
            Assert.Equal(0, _sut.FileDetail.TotalNumberofRecords);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_TotalFileAmountIsZero()
        {
            Assert.Equal(0, _sut.FileDetail.TotalFileAmount);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_CashLetterAmountIsZero()
        {
            Assert.Equal(0, _sut.CashLetterDetail.CashLetterAmount);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_CashLetterPositionIsZero()
        {
            Assert.Equal(0, _sut.CashLetterDetail.CashLetterPosition);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_CreditAmountIsZero()
        {
            Assert.Equal(0, _sut.CreditDetail.CreditAmount);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasANotNullPostingAccBankOnUs()
        {
            Assert.NotNull(_sut.CreditDetail.PostingAccBankOnUs);
        }

        [Fact]
        public void TestThat_AnEmptyRecord_HasANotNullPostingAccRt()
        {
            Assert.NotNull(_sut.CreditDetail.PostingAccRt);
        }
    }
}
