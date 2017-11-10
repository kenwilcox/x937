using Xunit;

namespace x937.Tests
{
    public class R26RecordTests
    {
        private readonly string _data;
        private readonly R26 _sut;

        public R26RecordTests()
        {
            var record = new Record("CheckDetailAddendumARecord", "26");
            _data = Builder.GetTestStringFor(record);
            _sut = new R26();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR26Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("26", _sut.RecordType);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_CheckDetailAddendumARecordNumber()
        {
            Assert.Equal("1", _sut.CheckDetailAddendumARecordNumber);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.BOFDRoutingNumber);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDBusinessDate()
        {
            Assert.Equal("YYYYMMDD", _sut.BOFDBusinessDate);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDItemSequenceNumber()
        {
            Assert.Equal("1 2 3 4 5 6 7 8", _sut.BOFDItemSequenceNumber);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDDepositAccountNumber()
        {
            Assert.Equal("------------------", _sut.BOFDDepositAccountNumber);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDDepositBranch()
        {
            Assert.Equal("-----", _sut.BOFDDepositBranch);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_PayeeName()
        {
            Assert.Equal("---------------", _sut.PayeeName);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_TruncationIndicator()
        {
            Assert.Equal("Y", _sut.TruncationIndicator);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDConversionIndicator()
        {
            Assert.Equal("2", _sut.BOFDConversionIndicator);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_BOFDCorrectionIndicator()
        {
            Assert.Equal("0", _sut.BOFDCorrectionIndicator);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_UserField()
        {
            Assert.Equal("-", _sut.UserField);
        }

        [Fact]
        public void TestThatR26_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("---", _sut.Reserved);
        }
    }
}
