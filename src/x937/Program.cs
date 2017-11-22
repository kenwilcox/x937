using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace x937
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Debug.WriteLine(args.Length);
            var records = Parser.ParseX9File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218.ICL"));
            var tree = BuildTree(records);

            var xml = new XmlDocument();
            xml.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"X9Fields.xml"));
            DumpData(tree, records, xml);
            var summary = Summary.GetSummary(records);
            Utils.Dump(summary.CreateNodeValues());
            WriteX9File(records, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218-new.ICL"));

            TranslateRecordData(records);
        }

        public static void WriteX9File(X9Recs records, string newX9File)
        {
            var fs = new FileStream(newX9File, FileMode.Create, FileAccess.Write, FileShare.Read);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var bw = new BinaryWriter(fs,Encoding.ASCII); //Encoding.GetEncoding(37));
            foreach (X9Rec item in records)
            {
                var data = new byte[0];
                if (item.ImageData?.Length > 0)
                {
                    data = item.ImageData;
                }

                var recLen = data.Length + item.RecData.Length;
                // Flip the bits
                var size = BitConverter.GetBytes(recLen);
                Array.Reverse(size);
                bw.Write(size);
                // Write the data in ASCII
                // This has to be converted to a char array, .net will add the size (pascal string) if it's just a string
                bw.Write(item.RecData.ToCharArray());
                bw.Write(data);
            }
            // Make sure it's all on disk before we bail.
            bw.Flush();
            fs.Flush();
            fs.Close();
        }

        public static TreeNode<string> BuildTree(X9Recs records)
        {
            TreeNode<string> clNode = null;
            TreeNode<string> bundleNode = null;
            TreeNode<string> checkNode = null;

            var tvx9 = new TreeNode<string>("");
            var index = 0;
            var checkCount = 0;
            foreach (X9Rec item in records)
            {
                switch (item.RecType)
                {
                    case "01": tvx9.Data = "Header (01):" + index++;break;
                    case "10": clNode = tvx9.AddChild("Cash Letter Header (10):" + index++); break;
                    case "20": bundleNode = clNode?.AddChild("Bundle Header (20):" + index++); break;
                    case "25": checkCount += 1; checkNode = bundleNode?.AddChild(checkCount.ToString("#,###,###") + ": Check Detail (25):" + index++); break;
                    case "26": checkNode?.AddChild("Addendum A (26):" + index++);break;
                    case "28": checkNode?.AddChild("Addendum C (28):" + index++);break;
                    case "31": checkNode?.AddChild("Return (31):" + index++);break;
                    case "32": checkNode?.AddChild("Return Addendum A (32):" + index++);break;
                    case "33": checkNode?.AddChild("Return Addendum B (33):" + index++);break;
                    case "35": checkNode?.AddChild("Return Addendum D (35):" + index++);break;
                    case "50": checkNode?.AddChild($"Image Detail {item.CheckImageType} (50):" + index++);break;
                    case "52": checkNode?.AddChild($"Image Data {item.CheckImageType} (52):" + index++);break;
                    case "61": checkNode = bundleNode?.AddChild("Credit/Reconcilation Record (61):" + index++);break;
                    case "70": bundleNode?.AddChild("Bundle Control (70):" + index++);break;
                    case "90": clNode?.AddChild("Cash Letter Control (90):" + index++);break;
                    case "99": tvx9.AddChild("File Control (99):" + index++);break;
                    default: Debug.WriteLine($"Unhandled case: {item.RecType}");break;
                }
            }
            return tvx9;
        }

        public static void DumpData(TreeNode<string> nodes, X9Recs records, XmlDocument xml)
        {
            // Move this to another method...
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x937-images");
            Directory.CreateDirectory(directory);
            foreach (var node in nodes)
            {
                var indent = Utils.CreateIndent(node.Level + 1);
                var bits = node.Data.Split(':');
                var index = int.Parse(bits[bits.Length - 1]);
                //var data = x9Stuff[index];
                //Console.WriteLine($"{indent}{node.Data} -- {data.recData}{(data.recImage.Length > 0 ? " -- Image@" : "")}{data.recImage}");//indent + (node.Data + ?? "null"));
                Console.WriteLine($"{indent}{node.Data}");
                DumpRecordData(records, index, indent, directory, xml);
            }

            //x9Stuff.Dump();
            Console.WriteLine("x9Stuff");
            foreach (X9Rec item in records)
            {
                Console.WriteLine($"{item.RecData}");
            }
        }

        private static void DumpRecordData(X9Recs records, int index, string indent, string directory, XmlNode doc)
        {
            var rec = records[index];
            if (!int.TryParse(rec.RecType, out int iRecType)) return;
            var fieldNodes = doc.SelectNodes("records/record[@type='" + rec.RecType + "']/field");
            if (fieldNodes != null)
            {
                foreach (XmlNode n in fieldNodes)
                {
                    var fieldStart = Convert.ToInt32(n.Attributes["start"].Value) - 1;
                    var fieldLen = Convert.ToInt32(n.Attributes["end"].Value) - fieldStart;
                    //dgvFields.Rows.Add(n.Attributes["name"].Value, rec.recData.Substring(fieldStart, fieldLen), n.Attributes["type"].Value, n.Attributes["comments"].Value);
                    Console.WriteLine($"{indent}{indent}{n.Attributes["name"].Value} = {rec.RecData.Substring(fieldStart, fieldLen)}"); //|{n.Attributes["type"].Value}|{n.Attributes["comments"].Value}");
                }
                //onSetImage(null, null);
            }

            if (iRecType != 52) return;
            var fname = $@"{index}-{rec.CheckImageType.ToString().ToLower()}.tiff";
            fname = Path.Combine(directory, fname);
            rec.FilePath = fname;
            File.WriteAllBytes(fname, rec.ImageData);
        }

        private static void TranslateRecordData(X9Recs records)
        {
            foreach (X9Rec item in records)
            {
                var rec = Translator.Translate(item);
                //if (rec is Unknown) continue;
                if (rec is Unknown) throw new Exception($"Missing Translation for {item.RecType}");
                var type = rec.GetType();
                Console.WriteLine($"Received: {type}");
                foreach (var prop in type.GetProperties())
                {
                    if (prop.PropertyType == typeof(byte[]))
                    {
                        Console.WriteLine($"{Utils.Prettify(prop.Name)} (byte[]) Length: {((byte[])prop.GetValue(rec)).Length}");
                    }
                    else
                    {
                        Console.WriteLine($"{Utils.Prettify(prop.Name)}: {prop.GetValue(rec)}");
                    }
                }
            }
        }
    }
}
