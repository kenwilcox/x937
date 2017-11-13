using Xunit;

namespace x937.Tests
{
    public class SummaryTests
    {
        private readonly Summary _sut;

        public SummaryTests()
        {
            var recs = new X9Recs
            {
                new X9Rec("01", "0103T111310346111310346201302182102NMY SILLY BANK NAMESTONEMOR LLC      1       ", ""),
                new X9Rec("10", "101211131034600090911320130218201302182102IG1       STONEMOR LLC  1234567890    ", ""),
                new X9Rec("20", "201211131034600090911320130218201302181         1   1                           ", ""),
                new X9Rec("61", "612001             540520053003921313200        0000011009121            G002   ", ""),
                new X9Rec("70", "702   0000000110090000000110094                                                 ", ""),
                new X9Rec("90", "901     2       000000000110094                                                 ", ""),
                new X9Rec("99", "991     21      2       0000000000011009                                        ", "")
            };

            _sut = Summary.GetSummary(recs);
        }

        [Fact]
        public void TestThat_ARecord_HasASummary()
        {
            Assert.NotNull(_sut);
        }

        [Fact]
        public void TestThat_ARecord_HasACashLetterSummary()
        {
            Assert.Single(_sut.CashLetterSummary);
        }

        [Fact]
        public void TestThat_ARecord_HasACreditSummary()
        {
            Assert.Single(_sut.CreditSummary);
        }

        [Fact]
        public void TestThat_ARecordFileDetail_HasABankName()
        {
            Assert.Equal("MY SILLY BANK NAME", _sut.FileDetail.BankName);
        }

        [Fact]
        public void TestThat_ARecordFileDetail_HasAFileCreationDate()
        {
            Assert.Equal("20130218", _sut.FileDetail.FileCreationDate);
        }

        [Fact]
        public void TestThat_ARecordFileDetail_HasAFileCreationTime()
        {
            Assert.Equal("2102", _sut.FileDetail.FileCreationTime);
        }

        [Fact]
        public void TestThat_ARecordFileDetail_HasATotalNumberofRecords()
        {
            Assert.Equal(21, _sut.FileDetail.TotalNumberofRecords);
        }

        [Fact]
        public void TestThat_ARecordFileDetail_HasATotalFileAmount()
        {
            Assert.Equal(11009, _sut.FileDetail.TotalFileAmount);
        }

        [Fact]
        public void TestThat_ARecordCashLetterSummary_HasACashLetterAmount()
        {
            Assert.Equal(11009, _sut.CashLetterSummary.TotalCashLetterAmount);
        }

        [Fact]
        public void TestThat_ARecordCreditSummary_HasATotalCreditRecordAmount()
        {
            Assert.Equal(11009, _sut.CreditSummary.TotalCreditRecordAmount);
        }
    }
}
