using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace x937.Tests
{
    public class X9RecsTest
    {
        [Fact]
        public void IsThisThingOn()
        {
            // Arrange, Act
            var rec = new X9Rec();
            var recs = new X9Recs {rec};

            // Assert
            Assert.Single(recs);
        }
    }
}
