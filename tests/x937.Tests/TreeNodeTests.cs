using System;
using Xunit;

namespace x937.Tests
{
    public class TreeNodeTests
    {
        [Fact]
        public void TestThatString_IsNotNull()
        {
            var node = new TreeNode<string>("Test");
            Assert.Equal("Test", node.Data);
        }

        [Fact]
        public void TestThatObjecString_IsNotNull()
        {
            // Arrange, Act
            var node = new TreeNode<object>("Test");
            // Assert
            Assert.NotNull(node.Data);
        }

        [Fact]
        public void TestThatClassWorks()
        {
            // Arrange, Act
            var test = new Test {Id = 1, Value = "One"};
            var node = new TreeNode<Test>(test);

            // Assert
            Assert.Equal(test.Id, node.Data.Id);
            Assert.Equal(test.Value, node.Data.Value);
        }

        [Fact]
        public void TestThatTreeNodeFind_ReturnsWhatYouSearchedFor()
        {
            // Arrange
            var test0 = new Test {Id = 0, Value = "Zero"};
            var test1 = new Test {Id = 1, Value = "One"};
            var test2 = new Test {Id = 2, Value = "Two"};

            // Act
            var node = new TreeNode<Test>(test0);
            node.AddChild(test1);
            node.AddChild(test2);

            // Assert
            var found = node.FindTreeNode(x => x.Data.Id == 2);
            Assert.Equal(test2.Id, found.Data.Id);
        }

        [Fact]
        public void TestThatTreeNode_HasChildren()
        {
            // Arrange
            var test0 = new Test {Id = 0, Value = "Zero"};
            var test1 = new Test {Id = 1, Value = "One"};
            var test2 = new Test {Id = 2, Value = "Two"};

            // Act
            var root = new TreeNode<Test>(test0);
            var branch = root.AddChild(test1);
            var leaf = branch.AddChild(test2);

            // Assert
            Assert.True(root.IsRoot);
            Assert.True(branch.IsBranch);
            Assert.True(leaf.IsLeaf);
        }

        [Fact]
        public void TestThatTreeNode_FindsAllMatchingChildren()
        {
            // Arrange
            var test0 = new Test {Id = 0, Value = "Zero"};
            var test1 = new Test {Id = 1, Value = "Zero"};
            var test2 = new Test {Id = 2, Value = "Zero"};

            // Act
            var root = new TreeNode<Test>(test0);
            var branch = root.AddChild(test1);
            branch.AddChild(test2);
            var found = root.FindAllTreeNodes(x => x.Data.Value == "Zero");

            // Assert
            Assert.Equal(3, found.Count);
        }

        [Fact]
        public void TestThatTreeBranch_HasChildren()
        {
            // Arrange
            var test0 = new Test {Id = 0, Value = "Zero"};
            var test1 = new Test {Id = 1, Value = "Zero"};
            var test2 = new Test {Id = 2, Value = "Zero"};

            // Act
            var root = new TreeNode<Test>(test0);
            var branch = root.AddChild(test1);
            branch.AddChild(test2);
            var found = branch.FindAllTreeNodes(x => x.Data.Value == "Zero");

            // Assert
            Assert.Equal(2, found.Count);
        }
    }

    internal class Test
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
