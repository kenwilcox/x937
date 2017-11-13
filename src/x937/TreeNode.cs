using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace x937
{
    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {
        public T Data { get; set; }
        public TreeNode<T> Parent { get; set; }
        public ICollection<TreeNode<T>> Children { get; }

        public bool IsRoot => Parent == null;

        public bool IsLeaf => Children.Count == 0;

        public bool IsBranch => Children.Count != 0;

        public int Level
        {
            get
            {
                if (IsRoot) return 0;
                return Parent.Level + 1;
            }
        }

        public TreeNode(T data)
        {
            Data = data;
            Children = new LinkedList<TreeNode<T>>();
            // Don't try to simplify object initializer, it breaks the code
            // Yes, all the crap below is to turn it all off
#pragma warning disable IDE0028 // Simplify collection initialization
            // ReSharper disable once UseObjectOrCollectionInitializer
            ElementsIndex = new LinkedList<TreeNode<T>>();
#pragma warning restore IDE0028 // Simplify collection initialization
            ElementsIndex.Add(this);
        }

        public TreeNode<T> AddChild(T child)
        {
            var childNode = new TreeNode<T>(child) {Parent = this};
            Children.Add(childNode);
            RegisterChildForSearch(childNode);
            return childNode;
        }

        public override string ToString()
        {
            return Data != null ? Data.ToString() : "[data null]";
        }

        #region searching

        private ICollection<TreeNode<T>> ElementsIndex { get; }

        private void RegisterChildForSearch(TreeNode<T> node)
        {
            ElementsIndex.Add(node);
            Parent?.RegisterChildForSearch(node);
        }

        public TreeNode<T> FindTreeNode(Func<TreeNode<T>, bool> predicate)
        {
            return ElementsIndex.FirstOrDefault(predicate);
        }

        public List<TreeNode<T>> FindAllTreeNodes(Func<TreeNode<T>, bool> predicate)
        {
            return ElementsIndex.Where(predicate).ToList();
        }

        #endregion

        #region iterating

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            yield return this;
            foreach (var directChild in Children)
            {
                foreach (var anyChild in directChild)
                    yield return anyChild;
            }
        }

        #endregion
    }
}
