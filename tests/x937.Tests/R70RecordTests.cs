using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class R70RecordTests
    {
        private readonly string _data;
        private readonly R70 _sut;

        public R70RecordTests()
        {
            var record = new XRecord("BatchControlRecord", "70");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R70();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR70Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("70", _sut.RecordType);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_ItemsWithinBatchCount()
        {
            Assert.Equal("0000", _sut.ItemsWithinBatchCount);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_BatchTotalAmount()
        {
            Assert.Equal("xxxxxxxxxxxx", _sut.BatchTotalAmount);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_MICRValidTotalAmount()
        {
            Assert.Equal("999999999999", _sut.MICRValidTotalAmount);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_ImagesWithinBatchCount()
        {
            Assert.Equal("-----", _sut.ImagesWithinBatchCount);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_UserField()
        {
            Assert.Equal("********************", _sut.UserField);
        }

        [Fact]
        public void TestThatR70_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("#########################", _sut.Reserved);
        }
    }
}
