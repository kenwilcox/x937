using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace x937.Tests
{
    public class MetaTests
    {
        private Meta meta;
        public MetaTests()
        {
            meta = Builder.GetMeta();
        }

        [Fact]
        public void TestFileHeaderRecord()
        {
            // Arrange
            var m01 = meta[new Record("FileHeaderRecord", "01")];
            var r01 = new R01();

            // Act
            var size = m01.Sum(x => x.Size);
            var props = r01.GetType().GetProperties().Select(x => x.Name).ToList();
            var mprops = m01.Select(x => x.FieldName).ToList();
            mprops.Sort();
            props.Sort();

            // Possibly overkill
            var list = new List<int>();
            foreach (var field in m01)
            {
                var start = field.Position.Start;
                var end = field.Position.End;
                for (var i = start; i < end; i++)
                    list.Add(i);
            }
            var mlist = new List<int>();
            for (var i = 0; i < size; i++)
                mlist.Add(i);


            // Assert
            Assert.Equal(80, size);
            Assert.Equal(mprops, props);
            Assert.Equal(80, list.Count);
            Assert.Equal(mlist, list);
        }

        [Fact]
        public void TestCashLetterHeaderRecord()
        {
            // Arrange
            var m10 = meta[new Record("CashLetterHeaderRecord", "10")];
            var r10 = new R10();

            // Act
            var size = m10.Sum(x => x.Size);
            var props = r10.GetType().GetProperties().Select(x => x.Name).ToList();
            var mprops = m10.Select(x => x.FieldName).ToList();
            mprops.Sort();
            props.Sort();

            // Possibly overkill
            var list = new List<int>();
            foreach (var field in m10)
            {
                var start = field.Position.Start;
                var end = field.Position.End;
                for (var i = start; i < end; i++)
                    list.Add(i);
            }
            var mlist = new List<int>();
            for (var i = 0; i < size; i++)
                mlist.Add(i);


            // Assert
            Assert.Equal(80, size);
            Assert.Equal(mprops, props);
            Assert.Equal(80, list.Count);
            Assert.Equal(mlist, list);
        }
    }
}
