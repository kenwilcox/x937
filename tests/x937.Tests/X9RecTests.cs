using Xunit;

namespace x937.Tests
{
    public class X9RecTests
    {
        [Fact]
        public void TestThat_WeAssignConnectlyInDefaultConstructor()
        {
            // Arrange, Act
            var rec = new X9Rec();

            // Assert
            Assert.NotNull(rec.RecType);
            Assert.NotNull(rec.RecData);
            Assert.NotNull(rec.RecImage);
        }

        [Fact]
        public void TestThat_WeAssignConnectlyInObjectInitializer()
        {
            // Arrange, Act
            var rec = new X9Rec{RecData = "Data", RecType="Type", RecImage = "Image"};

            // Assert
            Assert.Equal("Type", rec.RecType);
            Assert.Equal("Data", rec.RecData);
            Assert.Equal("Image", rec.RecImage);
        }

        [Fact]
        public void TestThat_WeAssignCorrectlyInConstructor()
        {
            // Arrange, Act
            var rec = new X9Rec("recType", "recData", "recImage");

            // Assert
            Assert.Equal("recType", rec.RecType);
            Assert.Equal("recData", rec.RecData);
            Assert.Equal("recImage", rec.RecImage);
        }
    }
}
