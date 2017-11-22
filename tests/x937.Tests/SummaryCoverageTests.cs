using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace x937.Tests
{
    public class SummaryCoverageTests
    {
        [Fact]
        public void Test_Summary_NodeValues_CoversEmptyCreditDetail()
        {
            // Arrange
            var recs = new X9Recs
            {
                new X9Rec("01", "0103T111310346111310346201302182102NMY SILLY BANK NAMESTONEMOR LLC      1       ", ""),
                new X9Rec("10", "101211131034600090911320130218201302182102IG1       STONEMOR LLC  1234567890    ", ""),
                new X9Rec("20", "201211131034600090911320130218201302181         1   1                           ", ""),
                // This removes the CreditDetail so there's none of that or CreditSummary
                //new X9Rec("61", "612001             540520053003921313200        0000011009121            G002   ", ""),
                new X9Rec("70", "702   0000000110090000000000004                                                 ", ""),
                new X9Rec("90", "901     2       000000000110094                                                 ", ""),
                new X9Rec("99", "991     21      2       0000000000011009                                        ", ""),
            };

            // Act
            var summary = Summary.GetSummary(recs);
            var sut = summary.CreateNodeValues();

            // Assert
            Assert.Equal(2, sut.Children.Count);
        }
    }
}
