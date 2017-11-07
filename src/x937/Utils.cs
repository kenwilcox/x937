using System;
using System.Text;

namespace x937
{
    public static class Utils
    {
        public static string CreateIndent(int depth)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < depth * 2; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }

        public static void Dump(TreeNode<string> nodes)
        {
            foreach (var node in nodes)
            {
                var indent = Utils.CreateIndent(node.Level+1);
                Console.WriteLine($"{indent}{node.Data}");
            }
        }
    }
}
