using x937.Meta;
using Xunit;

namespace x937.Tests
{
    public class R52RecordTests
    {
        private readonly string _data;
        private readonly byte[] _optional;
        private readonly R52 _sut;

        public R52RecordTests()
        {
            var record = new XRecord("ImageViewDataRecord", "52");
            var meta = Builder.GetMeta()[record];
            var data = Builder.GetTestStringFor(meta);
            _data = data.Substring(0, 117);
            _optional = System.Text.Encoding.ASCII.GetBytes(data.Substring(117));
            _sut = new R52();
            _sut.SetData(_data, _optional);
        }

        [Fact]
        public void TestThatR52Data_IsTheRightLength()
        {
            Assert.Equal(117, _data.Length);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_RecordType()
        {
            Assert.Equal("52", _sut.RecordType);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ECEInstitutionRoutingNumber()
        {
            Assert.Equal("TTTTAAAAC", _sut.ECEInstitutionRoutingNumber);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_BatchBusinessDate()
        {
            Assert.Equal("YYYYMMDD", _sut.BatchBusinessDate);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_CycleNumber()
        {
            Assert.Equal("--", _sut.CycleNumber);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ECEInstitutionItemSequenceNumber()
        {
            Assert.Equal("1 2 3 4 5 6 7 8", _sut.ECEInstitutionItemSequenceNumber);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_SecurityOriginatorName()
        {
            Assert.Equal("xxxxxxxxxxxxxxxx", _sut.SecurityOriginatorName);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_SecurityAuthenticator()
        {
            Assert.Equal("vvvvvvvvvvvvvvvv", _sut.SecurityAuthenticator);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_SecurityKeyName()
        {
            Assert.Equal("****************", _sut.SecurityKeyName);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ClippingOrigin()
        {
            Assert.Equal("0", _sut.ClippingOrigin);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ClippingCoordinateH1()
        {
            Assert.Equal("####", _sut.ClippingCoordinateH1);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ClippingCoordinateH2()
        {
            Assert.Equal("^^^^", _sut.ClippingCoordinateH2);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ClippingCoordinateV1()
        {
            Assert.Equal("xxxx", _sut.ClippingCoordinateV1);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_ClippingCoordinateV2()
        {
            Assert.Equal("----", _sut.ClippingCoordinateV2);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_LengthOfImageReferenceKey()
        {
            Assert.Equal("0000", _sut.LengthOfImageReferenceKey);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_LengthOfDigitalSignature()
        {
            Assert.Equal("00000", _sut.LengthOfDigitalSignature);
        }

        [Fact]
        public void TestThatR52_HasTheRightValueFor_LengthOfImageData()
        {
            var length = _sut.LengthOfImageData;
            var val = int.Parse(length);
            Assert.InRange(val, 2048, 65535);
        }

        [Fact]
        public void TestThatR52_IsTheRightSizeFor_ImageData()
        {
            var length = int.Parse(_sut.LengthOfImageData);
            Assert.Equal(length, _sut.ImageData.Length);
            Assert.Equal(length, _optional.Length);
        }
    }
}
