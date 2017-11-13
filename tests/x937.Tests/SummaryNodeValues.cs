using System.Linq;
using Xunit;

namespace x937.Tests
{
    public class SummaryNodeValues
    {
        public readonly TreeNode<string> _sut;

        public SummaryNodeValues()
        {
            var recs = new X9Recs
            {
                new X9Rec("01", "0103T111310346111310346201302182102NMY SILLY BANK NAMESTONEMOR LLC      1       ", ""),
                new X9Rec("10", "101211131034600090911320130218201302182102IG1       STONEMOR LLC  1234567890    ", ""),
                new X9Rec("20", "201211131034600090911320130218201302181         1   1                           ", ""),
                new X9Rec("61", "612001             540520053003921313200        0000011009121            G002   ", ""),
                new X9Rec("70", "702   0000000110090000000110094                                                 ", ""),
                new X9Rec("90", "901     2       000000000110094                                                 ", ""),
                new X9Rec("99", "991     21      2       0000000000011009                                        ", ""),
                new X9Rec("-1", "Booger", "") // dummy record, covers default section in switch
            };

            var summary = Summary.GetSummary(recs);
            _sut = summary.CreateNodeValues();
        }

        [Fact]
        public void TestSummary_NodeValues_IsNotNull()
        {
            Assert.NotNull(_sut);
        }

        [Fact]
        public void TestSummary_NodeValues_HasExpectedChildren()
        {
            Assert.Equal(3, _sut.Children.Count);
        }

        [Fact]
        public void Test_Summary_NodeValues_FirstElement_HasExpectedChildren()
        {
            Assert.Equal(3, _sut.Children.ElementAt(0).Children.Count);
        }

        [Fact]
        public void Test_Summary_NodeValues_SecondElement_HasExpectedChildren()
        {
            Assert.Equal(2, _sut.Children.ElementAt(1).Children.Count);
        }

        [Fact]
        public void Test_Summary_NodeValues_ThirdElement_HasExpectedChildren()
        {
            Assert.Equal(1, _sut.Children.ElementAt(2).Children.Count);
        }
    }
}
