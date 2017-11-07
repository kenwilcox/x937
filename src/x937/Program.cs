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
            var records = ParseX9File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218.ICL"));
            var tree = BuildTree(records);

            var xml = new XmlDocument();
            xml.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"X9Fields.xml"));
            DumpData(tree, records, xml);
            var summary = GetSummary(records);
            Utils.Dump(summary.CreateNodeValues());
            WriteX9File(records, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218-new.ICL"));
        }

        public static void WriteX9File(X9Recs records, string newX9File)
        {
            var fs = new FileStream(newX9File, FileMode.Create, FileAccess.Write, FileShare.Read);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var bw = new BinaryWriter(fs,Encoding.ASCII); //Encoding.GetEncoding(37));
            foreach (X9Rec item in records)
            {
                var data = new byte[0];
                if (File.Exists(item.FilePath))
                {
                    data = File.ReadAllBytes(item.FilePath);
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

        public static Summary GetSummary(X9Recs records)
        {
            var summary = new Summary();
            foreach(X9Rec item in records)
            {
                summary.SetSummaryData(item.RecType, item.RecData);
            }
            return summary;
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

        public static X9Recs ParseX9File(string x9File)
        {
            var ret = new X9Recs();

            // open x9.37 file from bank
            FileStream fs = new FileStream(x9File, FileMode.Open, FileAccess.Read, FileShare.Read);
            //Dim ofs As New FileStream(allImgFile, FileMode.Create)
            int curPos = 0;
            // 37 is EBCDIC encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // please???
            BinaryReader br = new BinaryReader(fs, Encoding.GetEncoding(37));
            //ArrayList alImg = new ArrayList();

            // Read first record
            var reclenB = br.ReadBytes(4); // to hold rec length in Big Endian byte order (motorola format) why Big Endian??
            curPos += 4;
            Array.Reverse(reclenB); // this is 'cause the rec length is in Big Endian order why? (probably some wise ass)
            Int32 reclen = BitConverter.ToInt32(reclenB, 0); // convert rec length to integer

            // variables to hold currect record
            string rec;

            // variables to hold various key lengths in the variable record type 53 which also holds the check image
            int refKeyLen;
            int sigLen;

            // counts
            int fileRecCount = 0;
            // Flags for start and end of sections
            bool fileStarted = false;
            bool clStarted = false;
            bool bundleStarted = false;
            bool fileEnded = false;
            bool clEnded = false;
            bool bundleEnded = false;
            bool checkStarted = false;
            bool checkFront50 = false;
            bool checkFront52 = false;
            bool checkBack50 = false;
            bool checkBack52 = false;

            // Loop thru the file
            while (reclen > 0 && !fileEnded)
            {
                //recB = new byte[reclen + 1];
                var recB = br.ReadBytes(reclen);
                curPos += reclen;
                if (fileRecCount % 100 == 0)
                {
                    // Do I care about displaying a progress?
                    //progbLoad.Value = (int)(readSize / (double)fileSize) * 100;
                    //Console.WriteLine((int)(readSize / (double)fileSize) * 100);
                    //Console.WriteLine((readSize / 1024.0).ToString("###,###,###,###") + " KB of " + (fileSize / 1024.0).ToString("###,###,###,###") + " KB");
                    //Application.DoEvents();
                }
                //rec = Encoding.ASCII.GetString(Encoding.Convert(Encoding.GetEncoding(37), Encoding.GetEncoding("ASCII"), recB));
                rec = Encoding.ASCII.GetString(recB);
                fileRecCount += 1;
                switch (rec.Substring(0, 2))
                {
                    case "01": // File Header record
                        fileStarted = true;
                        //_tvx9.Data = "Header (01):" +
                        ret.Add(new X9Rec("01", rec, ""));
                        //_onFileSummary(rec.Substring(0, 2), rec);
                        break;
                    case "10": // cash file header
                        if (fileStarted)
                        {
                            if (clStarted)
                            {
                                if (clEnded)
                                {
                                    clEnded = false;
                                }
                                else
                                {
                                    Console.Error.WriteLine("No Cash Letter Control record.");
                                    return new X9Recs();
                                }
                            }
                            else
                            {
                                clStarted = true;
                            }
                            ret.Add(new X9Rec("10", rec, ""));
                            //_onCashLetterSummary(rec.Substring(0, 2), rec);
                        }
                        else
                        {
                            // no file header yet
                            Console.Error.WriteLine("No File Header Record.");
                            return new X9Recs();
                        }
                        break;
                    case "20": // bundle header record
                        if (fileStarted)
                        {
                            if (clStarted)
                            {
                                if (bundleStarted)
                                {
                                    if (bundleEnded)
                                    {
                                        bundleEnded = false;
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("No Bundle Control record.");
                                        return new X9Recs();
                                    }
                                }
                                else
                                {
                                    bundleStarted = true;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("No Cash Letter Header record.");
                                return new X9Recs();
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("No File Header Record.");
                            return new X9Recs();
                        }
                        ret.Add(new X9Rec("20", rec, ""));
                        //_onBundleSummary(rec.Substring(0, 2), rec);
                        break;
                    case "25": // check detail record
                        if (bundleStarted && !bundleEnded)
                        {
                            if (checkStarted)
                            {
                                // make sure we got everything for previous check
                                if (!checkFront50)
                                {
                                    // no check front 50
                                    Console.Error.WriteLine("No Check Image Detail Record 50 - Front.");
                                    return new X9Recs();
                                }
                                if (!checkFront52)
                                {
                                    // no check front 52
                                    Console.Error.WriteLine("No Check Image Data Record 52 - Front.");
                                    return new X9Recs();
                                }
                                if (!checkBack50)
                                {
                                    // no check back 50
                                    Console.Error.WriteLine("No Check Image Detail Record 50 - Back.");
                                    return new X9Recs();
                                }
                                if (!checkBack52)
                                {
                                    // no check back 52
                                    Console.Error.WriteLine("No Check Image Data Record 52 - Back.");
                                    return new X9Recs();
                                }
                                checkBack50 = false;
                                checkBack52 = false;
                                checkFront50 = false;
                                checkFront52 = false;
                            }
                            else
                            {
                                checkStarted = true;
                            }
                        }
                        else
                        {
                            // no bundle header yet
                            Console.Error.WriteLine("No Bundle Header Record.");
                            return new X9Recs();
                        }
                        ret.Add(new X9Rec("25", rec, ""));
                        break;
                    case "26":
                        ret.Add(new X9Rec("26", rec, ""));
                        break;
                    case "28":
                        ret.Add(new X9Rec("28", rec, ""));
                        break;
                    case "31":
                        ret.Add(new X9Rec("31", rec, ""));
                        break;
                    case "32":
                        ret.Add(new X9Rec("32", rec, ""));
                        break;
                    case "33":
                        ret.Add(new X9Rec("33", rec, ""));
                        break;
                    case "35":
                        ret.Add(new X9Rec("35", rec, ""));
                        break;
                    case "50":
                        if (checkStarted)
                        {
                            if (checkFront50)
                            {
                                // back of check
                                checkBack50 = true;
                                ret.Add(new X9Rec("50", rec, "", CheckImage.Back, null));
                            }
                            else
                            {
                                // front of check
                                checkFront50 = true;
                                ret.Add(new X9Rec("50", rec, "", CheckImage.Front, null));
                            }
                        }
                        break;
                    case "52":
                        curPos -= reclen;
                        // read first 105 characters
                        rec = Encoding.ASCII.GetString(recB, 0, 105);
                        // get length of image reference key 102-105
                        refKeyLen = int.Parse(rec.Substring(101));
                        // read image ref key and digital sig length
                        rec = Encoding.ASCII.GetString(recB, 0, 105 + refKeyLen + 5);
                        sigLen = int.Parse(rec.Substring(105 + refKeyLen));
                        // read everything except image
                        rec = Encoding.ASCII.GetString(recB, 0, 105 + refKeyLen + 5 + sigLen + 7);
                        curPos += 105 + refKeyLen + 5 + sigLen + 7;
                        byte[] outArr = new byte[recB.GetUpperBound(0) - rec.Length + 1];
                        Array.Copy(recB, rec.Length, outArr, 0, recB.Length - rec.Length);
                        if (checkStarted)
                        {
                            if (checkFront52)
                            {
                                // back image of check
                                checkBack52 = true;
                                ret.Add(new X9Rec("52", rec, curPos + "," + outArr.Length, CheckImage.Back, outArr));
                            }
                            else
                            {
                                // front image of check
                                checkFront52 = true;
                                ret.Add(new X9Rec("52", rec, curPos + "," + outArr.Length, CheckImage.Front, outArr));
                            }
                        }
                        curPos += outArr.Length;
                        break;
                    case "61":
                        // read first 105 characters
                        if (bundleStarted && !bundleEnded)
                        {
                            ret.Add(new X9Rec("61", rec, ""));
                            checkStarted = true;
                            //_onCreditSummary(rec.Substring(0, 2), rec);
                            checkBack50 = false;
                            checkBack52 = false;
                            checkFront50 = false;
                            checkFront52 = false;
                        }
                        break;
                    case "70":
                        bundleEnded = true;
                        ret.Add(new X9Rec("70", rec, ""));
                        //_onBundleSummary(rec.Substring(0, 2), rec);
                        break;
                    case "90":
                        clEnded = true;
                        ret.Add(new X9Rec("90", rec, ""));
                        //_onCashLetterSummary(rec.Substring(0, 2), rec);
                        break;
                    case "99":
                        fileEnded = true;
                        ret.Add(new X9Rec("99", rec, ""));
                        //_onFileSummary(rec.Substring(0, 2), rec);
                        break;
                }
                reclenB = br.ReadBytes(4);
                curPos += 4;
                if (reclenB.Length == 4)
                {
                    Array.Reverse(reclenB);
                    reclen = BitConverter.ToInt32(reclenB, 0);
                }
                else
                {
                    reclen = 0;
                }
            }

            br.Close();
            fs.Close();

            return ret;
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
    }
}
