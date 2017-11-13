using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class R50RecordTests
    {
        private readonly string _data;
        private readonly R50 _sut;

        public R50RecordTests()
        {
            var record = new XRecord("ImageViewDetailRecord", "50");
            var meta = Builder.GetMeta()[record];
            _data = Builder.GetTestStringFor(meta);
            _sut = new R50();
            _sut.SetData(_data);
        }

        [Fact]
        public void TestThatR50Data_IsTheRightLength()
        {
            Assert.Equal(80, _data.Length);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("50", _sut.RecordType);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageIndicator()
        {
            Assert.Equal("1", _sut.ImageIndicator);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageCreatorRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ImageCreatorRoutingNumber);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageCreatorDate()
        {
            Assert.Equal("YYYYMMDD", _sut.ImageCreatorDate);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageViewFormatIndicator()
        {
            Assert.Equal("0 ", _sut.ImageViewFormatIndicator);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageViewCompressionAlgorithmIdentifier()
        {
            Assert.Equal("0 ", _sut.ImageViewCompressionAlgorithmIdentifier);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageViewDataSize()
        {
            Assert.Equal("-------", _sut.ImageViewDataSize);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ViewSideIndicator()
        {
            Assert.Matches(@"^[0|1]$", _sut.ViewSideIndicator);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ViewDescriptor()
        {
            Assert.Equal("0 ", _sut.ViewDescriptor);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_DigitalSignatureIndicator()
        {
            Assert.Equal("0", _sut.DigitalSignatureIndicator);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_DigitalSignatureMethod()
        {
            Assert.Equal("--", _sut.DigitalSignatureMethod);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_SecurityKeySize()
        {
            Assert.Equal("-----", _sut.SecurityKeySize);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_StartOfProtectedData()
        {
            Assert.Equal("-------", _sut.StartOfProtectedData);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_LengthOfProtectedData()
        {
            Assert.Equal("-------", _sut.LengthOfProtectedData);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_ImageRecreateIndicator()
        {
            Assert.Equal("-", _sut.ImageRecreateIndicator);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_UserField()
        {
            Assert.Equal("--------", _sut.UserField);
        }

        [Fact]
        public void TestThatR50_HasTheRightValueFor_Reserved()
        {
            Assert.Equal("---------------", _sut.Reserved);
        }
    }
}
