using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace x937.Tests
{
    public class TranslatorTests
    {
        [Fact]
        public void TestThatTranslator_Works()
        {
            // Arrange
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218.ICL");

            // Act
            var recs = Parser.ParseX9File(file);

            // Assert
            foreach(X9Rec rec in recs)
            {
                var obj = Translator.Translate(rec);
                Assert.NotNull(obj);
            }
        }

        [Fact]
        public void TestThatTranslate_Record26_Works()
        {
            // Arrange
            var rec = new X9Rec("26", "26------------------------------------------------------------------------------", "");

            // Act
            var obj = Translator.Translate(rec);

            // Assert
            Assert.NotNull(obj);
            Assert.True(obj is R26);
        }

        [Fact]
        public void TestThatTranslate_ReturnsUnknown()
        {
            // Arrange
            var rec = new X9Rec("-1", "-1-----", "");

            // Act
            var obj = Translator.Translate(rec);

            // Assert
            Assert.True(obj is Unknown);
        }

        [Theory]
        [InlineData("01", "01")] // Too short
        [InlineData("01", "01-------------------------------------------------------------------------------")] // Too Long

        public void TestThatTranslate_ThrowsException_WhenDataIsNotTheCorrectLength(string recType, string recData)
        {
            // Arrange
            var rec = new X9Rec(recType, recData, "");

            // Act
            var exception = Xunit.Record.Exception(() => Translator.Translate(rec));

            // Assert
            Assert.IsType<FormatException>(exception);
        }

        [Fact]
        public void TestThatTranslate_ThrowsArgumentOutOfRangeException_WhenThereIsNoData()
        {
            // Arrange
            var rec = new X9Rec();

            // Act
            var exception = Xunit.Record.Exception(() => Translator.Translate(rec));

            // Assert
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void TestThatTranslate_ThrowsArgumnetException_WhenTheTypesDoNotMatch()
        {
            // Arrange
            var rec = new X9Rec("01", "10------------------------------------------------------------------------------", "");

            // Act
            var exception = Xunit.Record.Exception(() => Translator.Translate(rec));

            // Assert
            Assert.IsType<ArgumentException>(exception);
            //Assert.True(exception is ArgumentException);
        }
    }
}
