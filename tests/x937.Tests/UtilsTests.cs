using System;
using System.IO;
using Xunit;

namespace x937.Tests
{
    public class UtilsTests
    {
        [Theory]
        [InlineData("HelloWorld", "Hello World")]
        [InlineData("SuperDuper", "Super Duper")]
        [InlineData("TestThatPrettifyWorks", "Test That Prettify Works")]
        public void TestThat_Prettify_Works(string input, string expected)
        {
            // Arrange, Act
            var pretty = Utils.Prettify(input);

            // Assert
            Assert.Equal(expected, pretty);
        }

        [Theory]
        [InlineData(1, "  ")]
        [InlineData(2, "    ")]
        [InlineData(3, "      ")]
        [InlineData(4, "        ")]
        [InlineData(5, "          ")]
        public void TestThat_CreateIndent_Works(int depth, string expected)
        {
            // Arrange, Act
            var indent = Utils.CreateIndent(depth);

            // Assert
            Assert.Equal(expected, indent);
        }

        [Fact]
        public void TestThat_UtilsDump_Outputs()
        {
            // Capture Console output, verify that it wrote

            // Arrange
            var tmp = Console.Out;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            Console.SetOut(sw);

            var tree = new TreeNode<string>("one");

            // Act
            Utils.Dump(tree);

            // reset and capture the data
            Console.SetOut(tmp);
            sw.Flush();
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var str = sr.ReadToEnd();

            // Assert
            Assert.NotNull(str);
            Assert.Contains("  one" + Environment.NewLine, str);
        }
    }
}
