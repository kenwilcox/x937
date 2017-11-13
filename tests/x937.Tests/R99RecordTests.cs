using Xunit;

namespace x937.Tests
{
    public class R99RecordTests
    {
        private readonly string _data;
        private readonly R99 _sut;

        public R99RecordTests()
        {
            var record = new Record("FileControlRecord", "99");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R99();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR99Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("99", _sut.RecordType);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_CashLetterCount()
        {
            Assert.Equal("123456", _sut.CashLetterCount);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_TotalRecordCount()
        {
            Assert.Equal("12345678", _sut.TotalRecordCount);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_TotalItemCount()
        {
            Assert.Equal("12345678", _sut.TotalItemCount);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_FileTotalAmount()
        {
            Assert.Equal("1234567890123456", _sut.FileTotalAmount);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_ImmediateOriginContactName()
        {
            Assert.Equal("6Z-5Y-4X-3W-2V", _sut.ImmediateOriginContactName);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_ImmediateOriginContactPhoneNumber()
        {
            Assert.Equal("1234567890", _sut.ImmediateOriginContactPhoneNumber);
        }

        [Fact]
        public void TestThatR99_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("----------------", _sut.Reserved);
        }
    }
}
