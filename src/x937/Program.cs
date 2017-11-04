using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace x937
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Debug.WriteLine(args.Length);
            var pgm = new Program();
            //pgm.ShowX9File(@"C:\Users\wilcoxk\Downloads\MTS_ICLViewer\Sample ICL File\101Bank Of America20130218.ICL");
            pgm.ShowX9File(@"101Bank Of America20130218.ICL");
        }

        public delegate void OnSummary(string recordType, string recData);

        public delegate void OnObjectRefresh();

        public X9Recs X9Stuff = new X9Recs();
        public XmlDocument XmlFields = new XmlDocument();
        private static TreeNode<string> _tvx9;
        private static OnSummary _onFileSummary;
        private static OnSummary _onCashLetterSummary;
        private static OnSummary _onCreditSummary;
        private static OnSummary _onBundleSummary;

        private static readonly Summary ObjSumary = new Summary();
        private FileStream _checkImageFs;
        private BinaryReader _checkImageBr;

        public void ShowX9File(string x9File)
        {
            XmlFields.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"X9Fields.xml"));

            _onFileSummary += ObjSumary.FileSummary;
            _onCashLetterSummary += ObjSumary.CashLetterSummary;
            _onCreditSummary += ObjSumary.CreditSummary;
            _onBundleSummary += ObjSumary.BundleSummary;

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
            int checkCount = 0;
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

            TreeNode<string> clNode = null;
            TreeNode<string> bundleNode = null;
            TreeNode<string> checkNode = null;
            //TreeNode creditNode = null;

            //long fileSize = fs.Length;
            _tvx9 = new TreeNode<string>("");
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
                        _tvx9.Data = "Header (01):" + X9Stuff.Add(new X9Rec("01", rec, ""));
                        _onFileSummary(rec.Substring(0, 2), rec);
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
                                    return;
                                }
                            }
                            else
                            {
                                clStarted = true;
                            }
                            clNode = _tvx9.AddChild("Cash Letter Header (10):" + X9Stuff.Add(new X9Rec("10", rec, "")));
                            _onCashLetterSummary(rec.Substring(0, 2), rec);
                        }
                        else
                        {
                            // no file header yet
                            Console.Error.WriteLine("No File Header Record.");
                            return;
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
                                        return;
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
                                return;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("No File Header Record.");
                            return;
                        }
                        bundleNode = clNode.AddChild("Bundle Header (20):" + X9Stuff.Add(new X9Rec("20", rec, "")));
                        _onBundleSummary(rec.Substring(0, 2), rec);
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
                                    return;
                                }
                                if (!checkFront52)
                                {
                                    // no check front 52
                                    Console.Error.WriteLine("No Check Image Data Record 52 - Front.");
                                    return;
                                }
                                if (!checkBack50)
                                {
                                    // no check back 50
                                    Console.Error.WriteLine("No Check Image Detail Record 50 - Back.");
                                    return;
                                }
                                if (!checkBack52)
                                {
                                    // no check back 52
                                    Console.Error.WriteLine("No Check Image Data Record 52 - Back.");
                                    return;
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
                            return;
                        }
                        checkCount += 1;
                        //if (Properties.Settings.Default.showItemNumber)
                        //{
                        //  checkNode = bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("25", rec, "")).ToString(), checkCount.ToString("#,###,###") + ": Check Detail (25)");
                        checkNode = bundleNode.AddChild(checkCount.ToString("#,###,###") + ": Check Detail (25):" + X9Stuff.Add(new X9Rec("25", rec, "")));
                        //}
                        //else
                        //{
                        //  checkNode = bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("25", rec, "")).ToString(), "Check Detail (25)");
                        //}
                        break;
                    case "26":
                        checkNode.AddChild("Addendum A (26):" + X9Stuff.Add(new X9Rec("26", rec, "")));
                        break;
                    case "28":
                        checkNode.AddChild("Addendum C (28):" + X9Stuff.Add(new X9Rec("28", rec, "")));
                        break;
                    case "31":
                        checkNode.AddChild("Return (31):" + X9Stuff.Add(new X9Rec("31", rec, "")));
                        break;
                    case "32":
                        checkNode.AddChild("Return Addendum A (32):" + X9Stuff.Add(new X9Rec("32", rec, "")));
                        break;
                    case "33":
                        checkNode.AddChild("Return Addendum B (33):" + X9Stuff.Add(new X9Rec("33", rec, "")));
                        break;
                    case "35":
                        checkNode.AddChild("Return Addendum D (35):" + X9Stuff.Add(new X9Rec("35", rec, "")));
                        break;
                    case "50":
                        if (checkStarted)
                        {
                            if (checkFront50)
                            {
                                // back of check
                                checkBack50 = true;
                                checkNode.AddChild("Image Detail Back (50):" + X9Stuff.Add(new X9Rec("50", rec, "")));
                            }
                            else
                            {
                                // front of check
                                checkFront50 = true;
                                checkNode.AddChild("Image Detail Front (50):" + X9Stuff.Add(new X9Rec("50", rec, "")));
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
                                checkNode.AddChild("Image Data Back (52):" + X9Stuff.Add(new X9Rec("52", rec, curPos + "," + outArr.Length)));
                            }
                            else
                            {
                                // front image of check
                                checkFront52 = true;
                                checkNode.AddChild("Image Data Front (52):" + X9Stuff.Add(new X9Rec("52", rec, curPos + "," + outArr.Length)));
                            }
                        }
                        curPos += outArr.Length;
                        break;
                    case "61":
                        // read first 105 characters
                        if (bundleStarted && !bundleEnded)
                        {
                            checkNode = bundleNode.AddChild("Credit/Reconcilation Record (61):" + X9Stuff.Add(new X9Rec("61", rec, "")));
                            checkStarted = true;
                            _onCreditSummary(rec.Substring(0, 2), rec);
                            checkBack50 = false;
                            checkBack52 = false;
                            checkFront50 = false;
                            checkFront52 = false;
                        }
                        break;
                    case "70":
                        bundleEnded = true;
                        bundleNode.AddChild("Bundle Control (70):" + X9Stuff.Add(new X9Rec("70", rec, "")));
                        _onBundleSummary(rec.Substring(0, 2), rec);
                        break;
                    case "90":
                        clEnded = true;
                        clNode.AddChild("Cash Letter Control (90):" + X9Stuff.Add(new X9Rec("90", rec, "")));
                        _onCashLetterSummary(rec.Substring(0, 2), rec);
                        break;
                    case "99":
                        fileEnded = true;
                        _tvx9.AddChild("File Control (99):" + X9Stuff.Add(new X9Rec("99", rec, "")));
                        _onFileSummary(rec.Substring(0, 2), rec);
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
            // GAAAH! Open up two additional Reader objects for use later?
            _checkImageFs = new FileStream(x9File, FileMode.Open, FileAccess.Read, FileShare.Read);
            _checkImageBr = new BinaryReader(_checkImageFs);
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x937-images");
            Directory.CreateDirectory(directory);
            foreach (var node in _tvx9)
            {
                var indent = Utils.CreateIndent(node.Level+1);
                var bits = node.Data.Split(':');
                var index = int.Parse(bits[bits.Length - 1]);
                //var data = x9Stuff[index];
                //Console.WriteLine($"{indent}{node.Data} -- {data.recData}{(data.recImage.Length > 0 ? " -- Image@" : "")}{data.recImage}");//indent + (node.Data + ?? "null"));
                Console.WriteLine($"{indent}{node.Data}");
                DumpRecordData(index, indent, directory);
            }

            //x9Stuff.Dump();
            Console.WriteLine("x9Stuff");
            foreach (X9Rec item in X9Stuff)
            {
                Console.WriteLine($"{item.RecData}");
            }
            ObjSumary.CreateNodeValues();
        }

        string _prev50 = "";

        void DumpRecordData(int index, string indent, string directory)
        {
            var rec = X9Stuff[index];
            if (!int.TryParse(rec.RecType, out int iRecType)) return;
            var fieldNodes = XmlFields.SelectNodes("records/record[@type='" + rec.RecType + "']/field");
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

            // Type 50
            //<field name="View Side Indicator" type="N" comments="The only valid values are: '0' Front Image View '1' Rear Image View" end="32" start="32"/>
            if (iRecType == 50)
            {
                _prev50 = rec.RecData.Substring(31, 1);
            }

            if (iRecType == 52)
            {
                var image = rec.RecImage;
                int startPos = Convert.ToInt32(image.Substring(0, image.IndexOf(",", StringComparison.Ordinal)));
                int imgLen = Convert.ToInt32(image.Substring(image.IndexOf(",", StringComparison.Ordinal) + 1));
                _checkImageBr.BaseStream.Seek(startPos, SeekOrigin.Begin);
                var recB = _checkImageBr.ReadBytes(imgLen); // new byte[imgLen + 1];
                //Byte2Image(ref fImg, recB, 0);
                //pbFront.Image = fImg;
                //fImg.Dump($"Front: {index}");
                //fImg.Save($@"C:\temp\x937\{index}-front.tiff");
                var fname = $@"{index}-{(_prev50 == "0" ? "front" : "back")}.tiff";
                fname = Path.Combine(directory, fname);
                File.WriteAllBytes(fname, recB);
                //var img = Image.FromFile(fname);
                //img.Dump();
            }
        }
    }
}
