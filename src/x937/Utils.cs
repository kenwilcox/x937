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
                var indent = CreateIndent(node.Level+1);
                Console.WriteLine($"{indent}{node.Data}");
            }
        }

        public static string Prettify(string word)
        {
            var sb = new StringBuilder();
            foreach (var c in word)
            {
                if (c.ToString() == c.ToString().ToUpper())
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            // terrible, but I'm lazy right now
            return sb.ToString().Replace("E C E", "ECE");
        }
    }
}
