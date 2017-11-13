using Xunit;

namespace x937.Tests
{
    public class R90RecordTests
    {
        private readonly string _data;
        private readonly R90 _sut;

        public R90RecordTests()
        {
            var record = new Record("CashLetterControlRecord", "90");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R90();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR90Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("90", _sut.RecordType);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_BatchCount()
        {
            Assert.Equal("999999", _sut.BatchCount);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_ItemsWithinCashLetterCount()
        {
            Assert.Equal("12345678", _sut.ItemsWithinCashLetterCount);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_CashLetterTotalAmount()
        {
            Assert.Equal("12345678901234", _sut.CashLetterTotalAmount);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_ImagesWithinCashLetterCount()
        {
            Assert.Equal("123456789", _sut.ImagesWithinCashLetterCount);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_CustomerName()
        {
            Assert.Equal("ABCDEFGHIJKLMNOPQR", _sut.CustomerName);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_CreditDate()
        {
            Assert.Equal("YYYYMMDD", _sut.CreditDate);
        }

        [Fact]
        public void TestThatR90_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("---------------", _sut.Reserved);
        }
    }
}
