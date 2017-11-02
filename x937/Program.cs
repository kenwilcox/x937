using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;

namespace x937
{
    public class Program
    {
        static void Main(string[] args)
        {
            var pgm = new Program();
            pgm.ShowX9File(@"C:\Users\wilcoxk\Downloads\MTS_ICLViewer\Sample ICL File\101Bank Of America20130218.ICL");
        }

        public delegate void OnSummary(string RecordType, string RecData);

        public delegate void OnObjectRefresh();

        public x9Recs x9Stuff = new x9Recs();
        public XmlDocument xmlFields = new XmlDocument();
        public TreeNode<string> tvx9;
        private static OnSummary onFileSummary;
        private static OnSummary onCashLetterSummary;
        private static OnSummary onCreditSummary;
        private static OnSummary onBundleSummary;
        private static OnObjectRefresh onObjectRefresh;
        private static OnObjectRefresh onObjectInitial;
        //private static OnObjectRefresh onCreateNodeValues;

        private static Summary _objSumary = new Summary();
        private FileStream checkImageFS;
        private BinaryReader checkImageBR;

        public void ShowX9File(string x9File)
        {
            // TODO This should be in the project
            xmlFields.Load(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"C:\temp\X9Fields.xml"));

            onFileSummary += new OnSummary(_objSumary.FileSummary);
            onCashLetterSummary += new OnSummary(_objSumary.CashLetterSummary);
            onCreditSummary += new OnSummary(_objSumary.CreditSummary);
            onBundleSummary += new OnSummary(_objSumary.BundleSummary);
            onObjectRefresh += new OnObjectRefresh(_objSumary.SetObjectsNewForm);
            //onCreateNodeValues += new OnObjectRefresh(_objSumary.CreateNodeValues);
            onObjectInitial += new OnObjectRefresh(_objSumary.SetObjects);

            // open x9.37 file from bank
            FileStream fs = new FileStream(x9File, FileMode.Open, FileAccess.Read, FileShare.Read);
            //Dim ofs As New FileStream(allImgFile, FileMode.Create)
            int curPos = 0;
            // 37 is EBCDIC encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // please???
            BinaryReader br = new BinaryReader(fs, System.Text.Encoding.GetEncoding(37));
            byte[] reclenB = new byte[4]; // to hold rec length in Big Endian byte order (motorola format) why Big Endian??
            ArrayList alImg = new ArrayList();
            int imgNbr = 0;

            // Read first record
            reclenB = br.ReadBytes(4); // first 4 bytes hold the record length
            curPos += 4;
            Array.Reverse(reclenB); // this is 'cause the rec length is in Big Endian order why? (probably some wise ass)
            Int32 reclen = BitConverter.ToInt32(reclenB, 0); // convert rec length to integer

            // variables to hold currect record
            byte[] recB = null;
            string rec = null;

            // variables to hold various key lengths in the variable record type 53 which also holds the check image
            int refKeyLen = 0;
            int sigLen = 0;
            int imgLen = 0;

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
            fileStarted = false; // Flag for header
            fileEnded = false; // flag for footer
            clStarted = false; // flag for cash letter header
            clEnded = false; // flag for cash letter footer
            bundleStarted = false; // flag for bundle header
            bundleEnded = false; // flage for bundle footer
            checkStarted = false;
            checkFront50 = false;
            checkFront52 = false;

            TreeNode<string> clNode = null;
            TreeNode<string> bundleNode = null;
            TreeNode<string> checkNode = null;
            //TreeNode creditNode = null;

            long fileSize = fs.Length;
            long readSize = 0;
//  lblFile.Text = "Loading ...";
//  tvX9.Nodes.Clear();
            tvx9 = new TreeNode<string>("");
            // Loop thru the file
            while (reclen > 0 && !fileEnded)
            {
                recB = new byte[reclen + 1];
                recB = br.ReadBytes(reclen);
                readSize += reclen + 4;
                curPos += reclen;
                if (fileRecCount % 100 == 0)
                {
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
                        //tvX9.Nodes.Add(x9Stuff.Add(new x9Rec("01", rec, "")).ToString(), "Header (01)").ForeColor = Properties.Settings.Default.color01;
                        tvx9.Data = "Header (01):" + x9Stuff.Add(new x9Rec("01", rec, "")).ToString();
                        onFileSummary(rec.Substring(0, 2), rec);
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
                                    //MessageBox.Show("No Cash Letter Control record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Console.Error.WriteLine("No Cash Letter Control record.");
                                    //lblFile.ForeColor = Color.Red;
                                    //lblFile.Text += " - Invalid file";
                                    return;
                                }
                            }
                            else
                            {
                                clStarted = true;
                            }
                            //clNode = tvX9.Nodes[0].Nodes.Add(x9Stuff.Add(new x9Rec("10", rec, "")).ToString(), "Cash Letter Header (10)");
                            clNode = tvx9.AddChild("Cash Letter Header (10):" + x9Stuff.Add(new x9Rec("10", rec, "")).ToString());
                            //clNode.ForeColor = Properties.Settings.Default.color10;
                            onCashLetterSummary(rec.Substring(0, 2), rec);
                        }
                        else
                        {
                            // no file header yet
                            //MessageBox.Show("No File Header Record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Console.Error.WriteLine("No File Header Record.");
                            //lblFile.ForeColor = Color.Red;
                            //lblFile.Text += " - Invalid file";
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
                                        //MessageBox.Show("No Bundle Control record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        Console.Error.WriteLine("No Bundle Control record.");
                                        //lblFile.ForeColor = Color.Red;
                                        //lblFile.Text += " - Invalid file";
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
                                //MessageBox.Show("No Cash Letter Header record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Console.Error.WriteLine("No Cash Letter Header record.");
                                //lblFile.ForeColor = Color.Red;
                                //lblFile.Text += " - Invalid file";
                                return;
                            }
                        }
                        else
                        {
                            // no file header yet
                            //MessageBox.Show("No File Header Record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Console.Error.WriteLine("No File Header Record.");
                            //lblFile.ForeColor = Color.Red;
                            //lblFile.Text += " - Invalid file";
                            return;
                        }
                        //bundleNode = clNode.Nodes.Add(x9Stuff.Add(new x9Rec("20", rec, "")).ToString(), "Bundle Header (20)");
                        bundleNode = clNode.AddChild("Bundle Header (20):" + x9Stuff.Add(new x9Rec("20", rec, "")).ToString());
                        //bundleNode.ForeColor = Properties.Settings.Default.color20;
                        onBundleSummary(rec.Substring(0, 2), rec);
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
                                    //MessageBox.Show("No Check Image Detail Record 50 - Front.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Console.Error.WriteLine();
                                    //lblFile.ForeColor = Color.Red;
                                    //lblFile.Text += " - Invalid file";
                                    return;
                                }
                                if (!checkFront52)
                                {
                                    // no check front 52
                                    //MessageBox.Show("No Check Image Data Record 52 - Front.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Console.Error.WriteLine();
                                    //lblFile.ForeColor = Color.Red;
                                    //lblFile.Text += " - Invalid file";
                                    return;
                                }
                                if (!checkBack50)
                                {
                                    // no check back 50
                                    //MessageBox.Show("No Check Image Detail Record 50 - Back.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Console.Error.WriteLine();
                                    //lblFile.ForeColor = Color.Red;
                                    //lblFile.Text += " - Invalid file";
                                    return;
                                }
                                if (!checkBack52)
                                {
                                    // no check back 52
                                    //MessageBox.Show("No Check Image Data Record 52 - Back.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Console.Error.WriteLine();
                                    //lblFile.ForeColor = Color.Red;
                                    //lblFile.Text += " - Invalid file";
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
                            //MessageBox.Show("No Bundle Header Record.", "Error MTS.ICLViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Console.Error.WriteLine();
                            //lblFile.ForeColor = Color.Red;
                            //lblFile.Text += " - Invalid file";
                            return;
                        }
                        checkCount += 1;
                        //if (Properties.Settings.Default.showItemNumber)
                        //{
                        //  checkNode = bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("25", rec, "")).ToString(), checkCount.ToString("#,###,###") + ": Check Detail (25)");
                        checkNode = bundleNode.AddChild(checkCount.ToString("#,###,###") + ": Check Detail (25):" + x9Stuff.Add(new x9Rec("25", rec, "")).ToString());
                        //}
                        //else
                        //{
                        //  checkNode = bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("25", rec, "")).ToString(), "Check Detail (25)");
                        //}
                        //checkNode.ForeColor = Properties.Settings.Default.color25;
                        break;
                    case "26":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("26", rec, "")).ToString(), "Addendum A (26)").ForeColor = Color.Green;
                        checkNode.AddChild("Addendum A (26):" + x9Stuff.Add(new x9Rec("26", rec, "")).ToString());
                        break;
                    case "28":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("28", rec, "")).ToString(), "Addendum C (28)").ForeColor = Color.Green;
                        checkNode.AddChild("Addendum C (28):" + x9Stuff.Add(new x9Rec("28", rec, "")).ToString());
                        break;
                    case "31":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("31", rec, "")).ToString(), "Return (31)").ForeColor = Color.Green;
                        checkNode.AddChild("Return (31):" + x9Stuff.Add(new x9Rec("31", rec, "")).ToString());
                        break;
                    case "32":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("32", rec, "")).ToString(), "Return Addendum A (32)").ForeColor = Color.Green;
                        checkNode.AddChild("Return Addendum A (32):" + x9Stuff.Add(new x9Rec("32", rec, "")).ToString());
                        break;
                    case "33":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("33", rec, "")).ToString(), "Return Addendum B (33)").ForeColor = Color.Green;
                        checkNode.AddChild("Return Addendum B (33):" + x9Stuff.Add(new x9Rec("33", rec, "")).ToString());
                        break;
                    case "35":
                        //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("35", rec, "")).ToString(), "Return Addendum D (35)").ForeColor = Color.Green;
                        checkNode.AddChild("Return Addendum D (35):" + x9Stuff.Add(new x9Rec("35", rec, "")).ToString());
                        break;
                    case "50":
                        if (checkStarted)
                        {
                            if (checkFront50)
                            {
                                // back of check
                                checkBack50 = true;
                                //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("50", rec, "")).ToString(), "Image Detail Back (50)").ForeColor = Properties.Settings.Default.color50;
                                checkNode.AddChild("Image Detail Back (50):" + x9Stuff.Add(new x9Rec("50", rec, "")).ToString());
                            }
                            else
                            {
                                // front of check
                                checkFront50 = true;
                                //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("50", rec, "")).ToString(), "Image Detail Front (50)").ForeColor = Properties.Settings.Default.color50;
                                checkNode.AddChild("Image Detail Front (50):" + x9Stuff.Add(new x9Rec("50", rec, "")).ToString());
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
                        imgLen = int.Parse(rec.Substring(rec.Length - 7));
                        byte[] outArr = new byte[recB.GetUpperBound(0) - rec.Length + 1];
                        Array.Copy(recB, rec.Length, outArr, 0, recB.Length - rec.Length);
                        if (checkStarted)
                        {
                            if (checkFront52)
                            {
                                // back image of check
                                checkBack52 = true;
                                //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("52", rec, curPos.ToString() + "," + outArr.Length.ToString())).ToString(), "Image Data Back (52)").ForeColor = Properties.Settings.Default.color52;
                                checkNode.AddChild("Image Data Back (52):" + x9Stuff.Add(new x9Rec("52", rec, curPos.ToString() + "," + outArr.Length.ToString())).ToString());
                            }
                            else
                            {
                                // front image of check
                                checkFront52 = true;
                                //checkNode.Nodes.Add(x9Stuff.Add(new x9Rec("52", rec, curPos.ToString() + "," + outArr.Length.ToString())).ToString(), "Image Data Front (52)").ForeColor = Properties.Settings.Default.color52;
                                checkNode.AddChild("Image Data Front (52):" + x9Stuff.Add(new x9Rec("52", rec, curPos.ToString() + "," + outArr.Length.ToString())).ToString());
                            }
                        }
                        imgNbr += 1;
                        curPos += outArr.Length;
                        break;
                    case "61":
                        // read first 105 characters
                        if (bundleStarted && !bundleEnded)
                        {
                            //checkNode = bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("61", rec, "")).ToString(), "Credit/Reconcilation Record (61)");
                            checkNode = bundleNode.AddChild("Credit/Reconcilation Record (61):" + x9Stuff.Add(new x9Rec("61", rec, "")).ToString());
                            //checkNode.ForeColor = Properties.Settings.Default.color61;
                            checkStarted = true;
                            onCreditSummary(rec.Substring(0, 2), rec);
                            checkBack50 = false;
                            checkBack52 = false;
                            checkFront50 = false;
                            checkFront52 = false;
                        }
                        break;
                    case "70":
                        bundleEnded = true;
                        //bundleNode.Nodes.Add(x9Stuff.Add(new x9Rec("70", rec, "")).ToString(), "Bundle Control (70)").ForeColor = Properties.Settings.Default.color20;
                        bundleNode.AddChild("Bundle Control (70):" + x9Stuff.Add(new x9Rec("70", rec, "")).ToString());
                        onBundleSummary(rec.Substring(0, 2), rec);
                        break;
                    case "90":
                        clEnded = true;
                        //clNode.Nodes.Add(x9Stuff.Add(new x9Rec("90", rec, "")).ToString(), "Cash Letter Control (90)").ForeColor = Properties.Settings.Default.color10;
                        clNode.AddChild("Cash Letter Control (90):" + x9Stuff.Add(new x9Rec("90", rec, "")).ToString());
                        onCashLetterSummary(rec.Substring(0, 2), rec);
                        break;
                    case "99":
                        fileEnded = true;
                        //tvX9.Nodes.Add(x9Stuff.Add(new x9Rec("99", rec, "")).ToString(), "File Control (99)").ForeColor = Properties.Settings.Default.color01;
                        tvx9.AddChild("File Control (99):" + x9Stuff.Add(new x9Rec("99", rec, "")).ToString());
                        onFileSummary(rec.Substring(0, 2), rec);
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
            // GAAAH!
            checkImageFS = new FileStream(x9File, FileMode.Open, FileAccess.Read, FileShare.Read);
            checkImageBR = new BinaryReader(checkImageFS);

            foreach (var node in tvx9)
            {
                var indent = CreateIndent(node.Level);
                var bits = node.Data.Split(':');
                var index = int.Parse(bits[bits.Length - 1]);
                //var data = x9Stuff[index];
                //Console.WriteLine($"{indent}{node.Data} -- {data.recData}{(data.recImage.Length > 0 ? " -- Image@" : "")}{data.recImage}");//indent + (node.Data + ?? "null"));
                Console.WriteLine($"{indent}{node.Data}");
                DumpRecordData(index, indent);
            }

            //x9Stuff.Dump();
            Console.WriteLine("x9Stuff");
            foreach (x9Rec item in x9Stuff)
            {
                Console.WriteLine($"{item.recData}");
            }
            _objSumary.CreateNodeValues();
        }

        string prev50 = "";

        void DumpRecordData(int index, string indent)
        {
            var rec = x9Stuff[index];
            if (!int.TryParse(rec.recType, out int iRecType)) return;
            XmlNodeList fieldNodes = null;
            int fieldStart = 0;
            int fieldLen = 0;
            fieldNodes = xmlFields.SelectNodes("records/record[@type='" + rec.recType + "']/field");
            if (fieldNodes != null)
            {
                foreach (XmlNode n in fieldNodes)
                {
                    fieldStart = System.Convert.ToInt32(n.Attributes["start"].Value) - 1;
                    fieldLen = System.Convert.ToInt32(n.Attributes["end"].Value) - fieldStart;
                    //dgvFields.Rows.Add(n.Attributes["name"].Value, rec.recData.Substring(fieldStart, fieldLen), n.Attributes["type"].Value, n.Attributes["comments"].Value);
                    Console.WriteLine($"{indent}{indent}{n.Attributes["name"].Value} = {rec.recData.Substring(fieldStart, fieldLen)}"); //|{n.Attributes["type"].Value}|{n.Attributes["comments"].Value}");
                }
                //onSetImage(null, null);
            }

            // Type 50
            //<field name="View Side Indicator" type="N" comments="The only valid values are: '0' Front Image View '1' Rear Image View" end="32" start="32"/>
            if (iRecType == 50)
            {
                prev50 = rec.recData.Substring(31, 1);
            }

            if (iRecType == 52)
            {
                var image = rec.recImage;
                int startPos = System.Convert.ToInt32(image.Substring(0, image.IndexOf(",")));
                int imgLen = System.Convert.ToInt32(image.Substring(image.IndexOf(",") + 1));
                byte[] recB = new byte[imgLen + 1];
                checkImageBR.BaseStream.Seek(startPos, SeekOrigin.Begin);
                recB = checkImageBR.ReadBytes(imgLen);
                //Byte2Image(ref fImg, recB, 0);
                //pbFront.Image = fImg;
                //fImg.Dump($"Front: {index}");
                //fImg.Save($@"C:\temp\x937\{index}-front.tiff");
                var fname = $@"C:\temp\x937\{index}-{(prev50 == "0" ? "front" : "back")}.tiff";
                File.WriteAllBytes(fname, recB);
                //var img = Image.FromFile(fname);
                //img.Dump();
            }

//            return;
//            if (iRecType >= 25 && iRecType < 70) // && Properties.Settings.Default.checkImage)
//            {
//                int curRec = index;
//                string frontImg = null;
//                string backImg = null;
//                //int.TryParse(index, out curRec);
//                x9Rec imgRec = x9Stuff[curRec];
//                while (!(imgRec.recType == "25" || imgRec.recType == "61" || curRec < 0))
//                {
//                    curRec -= 1;
//                    imgRec = x9Stuff[curRec];
//                }
//                if (curRec < 0 || curRec == index)
//                {
//                    return;
//                }
//                if (curRec >= 0)
//                {
//                    //tvX9_AfterSelect_cur25Rec = curRec;
//                    // move forward to first 52
//                    while (!(imgRec.recType == "52" || curRec > x9Stuff.Count))
//                    {
//                        curRec += 1;
//                        imgRec = x9Stuff[curRec];
//                    }
//                    if (curRec < x9Stuff.Count)
//                    {
//                        frontImg = imgRec.recImage;
//                        // move forward to second 52
//                        curRec += 1;
//                        imgRec = x9Stuff[curRec];
//                        while (!(imgRec.recType == "52" || curRec > x9Stuff.Count))
//                        {
//                            curRec += 1;
//                            imgRec = x9Stuff[curRec];
//                        }
//                        if (curRec < x9Stuff.Count)
//                        {
//                            backImg = imgRec.recImage;
//                        }
//                    }
//                }
////    if (tcX9.TabPages.Count < 2)
////    {
////      tcX9.TabPages.Add(tpImg);
////    }
////    pbCheck.Visible = false;
////    pbFront.Visible = true;
//                //Image fImg = null;
//                //Image bImg = null;
//                int startPos = System.Convert.ToInt32(frontImg.Substring(0, frontImg.IndexOf(",")));
//                int imgLen = System.Convert.ToInt32(frontImg.Substring(frontImg.IndexOf(",") + 1));
//                byte[] recB = new byte[imgLen + 1];
//                checkImageBR.BaseStream.Seek(startPos, SeekOrigin.Begin);
//                recB = checkImageBR.ReadBytes(imgLen);
//                //Byte2Image(ref fImg, recB, 0);
//                //pbFront.Image = fImg;
//                //fImg.Dump($"Front: {index}");
//                //fImg.Save($@"C:\temp\x937\{index}-front.tiff");
//                File.WriteAllBytes($@"C:\temp\x937\{index}-front.tiff", recB);
//                //pbBack.Visible = true;
//                startPos = System.Convert.ToInt32(backImg.Substring(0, backImg.IndexOf(",")));
//                imgLen = System.Convert.ToInt32(backImg.Substring(backImg.IndexOf(",") + 1));
//                recB = new byte[imgLen + 1];
//                checkImageBR.BaseStream.Seek(startPos, SeekOrigin.Begin);
//                recB = checkImageBR.ReadBytes(imgLen);
//                //Byte2Image(ref bImg, recB, 0);
//                //bBack.Image = bImg;
//                //bImg.Dump($"Back: {index}");
//                File.WriteAllBytes($@"C:\temp\x937\{index}-back.tiff", recB);
////    try
////    {
////      bImg.Save($@"C:\temp\x937\{index}-back.tiff");
////    }
////    catch
////    {
////      Console.Error.WriteLine("-------------------------------------------------------------Error Exporting Back Image");
////    }
////    onSetImage(fImg, bImg);
//            }
//            else
//            {
//                // I don't think I need to do any of this stuff;
////    //pbBack.Visible = false;
////    //pbFront.Visible = false;
////    //pbCheck.Visible = true;
////    if (rec.recImage.Length == 0)
////    {
////      if (tcX9.TabPages.Count > 1)
////      {
////        tcX9.TabPages.RemoveAt(1);
////      }
////    }
////    else
////    {
////      if (tcX9.TabPages.Count < 2)
////      {
////        tcX9.TabPages.Add(tpImg);
////      }
////      Image cImg = null;
////      int startPos = System.Convert.ToInt32(rec.recImage.Substring(0, rec.recImage.IndexOf(",")));
////      int imgLen = System.Convert.ToInt32(rec.recImage.Substring(rec.recImage.IndexOf(",") + 1));
////      byte[] recB = new byte[imgLen + 1];
////      checkImageBR.BaseStream.Seek(startPos, SeekOrigin.Begin);
////      recB = checkImageBR.ReadBytes(imgLen);
////      Byte2Image(ref cImg, recB, 0);
////      pbFront.Image = cImg;
////    }
//            }
        }

/*
public void SaveTiffintoSingle(string outFile, EncoderValue compressEncoder)
{
  //use for save image as single tiff
  int frame = 0;
  int startPos = 0;
  int imgLen = 0;
  byte[] recB = null;

  bool frontImage = false;
  int reccnt = 0;
  string SingleTiffImagePath = string.Empty;
  try
  {
    foreach (x9Rec rec in x9Stuff)
    {
      if (rec.recType == "50" && rec.recData.Substring(31, 1) == "0")
      {
        frontImage = true;
      }
      else if (rec.recType == "50" && rec.recData.Substring(31, 1) == "1")
      {
        frontImage = false;
      }
      if (rec.recImage.Trim().Length > 0)
      {
        startPos = System.Convert.ToInt32(rec.recImage.Substring(0, rec.recImage.IndexOf(",")));
        imgLen = System.Convert.ToInt32(rec.recImage.Substring(rec.recImage.IndexOf(",") + 1));
        checkImageBR.BaseStream.Seek(startPos, SeekOrigin.Begin);
        recB = new byte[imgLen + 1];
        recB = checkImageBR.ReadBytes(imgLen);
        if (!Directory.Exists(outFile))
          Directory.CreateDirectory(outFile);
        if (frontImage)
          SingleTiffImagePath = frame + "F.tiff";
        else
        {
          SingleTiffImagePath = frame + "R.tiff";
          frame += 1;
        }
        if (File.Exists(Path.Combine(outFile, SingleTiffImagePath)))
          File.Delete(Path.Combine(outFile, SingleTiffImagePath));
        File.WriteAllBytes(Path.Combine(outFile, SingleTiffImagePath), recB);
      }
      reccnt += 1;
    }
  }
  catch (Exception ex)
  {
    //MessageBox.Show(ex.Message + " rec count=" + reccnt.ToString("###,###,###"), "Error Exporting", MessageBoxButtons.OK, MessageBoxIcon.Error);
    Console.Error.WriteLine(ex.Message + " rec count=" + reccnt.ToString("###,###,###"));
  }
}
*/

//public void Byte2Image(ref Image NewImage, byte[] ByteArr, int startIndex)
//{
//  MemoryStream ImageStream = null;
//  byte[] newArr = new byte[ByteArr.GetUpperBound(0) - startIndex + 1];
//  Array.Copy(ByteArr, startIndex, newArr, 0, ByteArr.Length - startIndex);
//  try
//  {
//    if (newArr.GetUpperBound(0) > 0)
//    {
//      ImageStream = new MemoryStream(newArr);
//      NewImage = Image.FromStream(ImageStream);
//    }
//    else
//    {
//      NewImage = null;
//    }
//    ImageStream.Close();
//  }
//  catch// (Exception ex)
//  {
//    NewImage = null;
//  }
//}

        string CreateIndent(int depth)
        {
            depth += 1;
            var sb = new StringBuilder();
            for (var i = 0; i < depth * 2; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }

    public class x9Rec
    {
        private string _recType;
        private string _recData;
        private string _recImage;

        public x9Rec()
        {

        }

        public x9Rec(string recType, string recData, string recImage)
        {
            _recType = recType;
            _recData = recData;
            _recImage = recImage;
        }

        public string recType
        {
            get { return _recType; }
            set { _recType = value; }
        }

        public string recData
        {
            get { return _recData; }
            set { _recData = value; }
        }

        public string recImage
        {
            get { return _recImage; }
            set { _recImage = value; }
        }

    }

    public class x9Recs : System.Collections.CollectionBase
    {
        public int Add(x9Rec newX9Rec)
        {
            return List.Add(newX9Rec);
        }

        public void Remove(int index = -1)
        {
            if (index == -1)
            {
                index = List.Count - 1;
            }
            if (index >= 0 && index < List.Count)
            {
                List.RemoveAt(index);
            }
        }

        public x9Rec this[int index]
        {
            get { return (x9Rec) (List[index]); }
        }
    }

//public class CheckImage
//{

//  public void SetImage(Image fimage, Image rimage)
//  {
//    if (fimage == null || rimage == null) return;
//    Console.WriteLine($"fimage size {fimage.Size}");
//    Console.WriteLine($"rimage size {rimage.Size}");
//  }
//}

    public class ICLBase<T> : List<T>
    {

    }

    public sealed class ICLFileSummary : ICLBase<ICLFileDetail>
    {

    }

    public sealed class ICLCashLetterSummary : ICLBase<ICLCashLetterDetail>
    {
        public Int64 TotalCashLetterAmount
        {
            get
            {
                Int64 _amt = 0;
                foreach (ICLCashLetterDetail cld in this)
                {
                    _amt += cld.CashLetterAmount;
                }
                return _amt;
            }
        }
    }

    public sealed class ICLCreditSummary : ICLBase<ICLCreditDetail>
    {
        public Int64 TotalCreditRecordAmount
        {
            get
            {
                Int64 _amt = 0;
                foreach (ICLCreditDetail Cd in this)
                {
                    _amt += Cd.CreditAmount;
                }
                return _amt;
            }
        }
    }

    public class ICLFileDetail
    {
        public string BankName { get; set; }
        public int TotalNumberofRecords { get; set; }
        public Int64 TotalFileAmount { get; set; }
        public string FileCreationDate { get; set; }
        public string FileCreationTime { get; set; }
    }

    public class ICLCashLetterDetail
    {
        public int CashLetterPosition { get; set; }
        public Int64 CashLetterAmount { get; set; }
    }

    public class ICLCreditDetail
    {
        public string PostingAccRT { get; set; }
        public string PostingAccBankOnUs { get; set; }
        public Int64 CreditAmount { get; set; }
    }

    public class Summary
    {
        private static ICLFileSummary _objFileSummary;
        private static ICLCashLetterSummary _objCashLetterSummary;
        private static ICLCreditSummary _objCreditSummary;

        private static ICLFileDetail _ifd;
        private static ICLCashLetterDetail _icld;
        private static ICLCreditDetail _icrd;

        public Summary()
        {
            try
            {
                SetObjects();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetObjectsNewForm()
        {
            //Debug.WriteLine("SetObjectsNewForm");
            _ifd = new ICLFileDetail();
            _icld = new ICLCashLetterDetail();
            _icrd = new ICLCreditDetail();
            _objFileSummary = new ICLFileSummary();
            _objCashLetterSummary = new ICLCashLetterSummary();
            _objCreditSummary = new ICLCreditSummary();
        }

        public void SetObjects()
        {
            //Debug.WriteLine("SetObjects");
            _ifd = _ifd == null ? new ICLFileDetail() : _ifd;
            _icld = _icld == null ? new ICLCashLetterDetail() : _icld;
            _icrd = _icrd == null ? new ICLCreditDetail() : _icrd;
            _objFileSummary = _objFileSummary == null ? new ICLFileSummary() : _objFileSummary;
            _objCashLetterSummary = _objCashLetterSummary == null ? new ICLCashLetterSummary() : _objCashLetterSummary;
            _objCreditSummary = _objCreditSummary == null ? new ICLCreditSummary() : _objCreditSummary;
        }

        private void CashLetterEnd()
        {
            _objCashLetterSummary.Add(_icld);
            _icld = new ICLCashLetterDetail();
        }

        private void BundleEnd()
        {
            if (_icrd != null && _icrd.CreditAmount > 0)
            {
                _objCreditSummary.Add(_icrd);
                _icrd = new ICLCreditDetail();
            }
        }

        public void FileSummary(string RecType, string RecData)
        {
            if (RecType == "01")
            {
                _ifd.BankName = RecData.Substring(36, 18);
                _ifd.FileCreationDate = RecData.Substring(23, 8);
                _ifd.FileCreationTime = RecData.Substring(31, 4);

            }
            if (RecType == "99")
            {
                _ifd.TotalFileAmount = Convert.ToInt64(RecData.Substring(24, 16));
                _ifd.TotalNumberofRecords = Convert.ToInt32(RecData.Substring(8, 8));
            }
        }

        public void CashLetterSummary(string RecType, string RecData)
        {
            if (RecType == "10" || RecType == "90")
            {
                if (RecType == "10")
                {
                    _icld.CashLetterPosition = Convert.ToInt32(RecData.Substring(44, 8));
                }
                if (RecType == "90")
                {
                    _icld.CashLetterAmount = Convert.ToInt64(RecData.Substring(16, 14));
                    CashLetterEnd();
                }
            }
        }

        public void BundleSummary(string RecType, string RecData)
        {
            if (RecType == "20" || RecType == "70")
            {
                if (RecType == "20")
                {
                }
                if (RecType == "70")
                {
                    BundleEnd();
                }
            }
        }

        public void CreditSummary(string RecType, string RecData)
        {
            if (RecType == "61")
            {
                _icrd.CreditAmount = Convert.ToInt64(RecData.Substring(48, 10));
                _icrd.PostingAccRT = RecData.Substring(19, 9);
                _icrd.PostingAccBankOnUs = RecData.Substring(28, 20);
            }
        }

        private void Summary_Load(object sender, EventArgs e)
        {
            try
            {
                CreateNodeValues();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateNodeValues()
        {
            Debug.WriteLine("CreateNodeValues");
            Console.WriteLine("**SUMMARY**");
            var tvSum = new TreeNode<string>(_ifd.BankName.Trim() + " - ICL File Summary");
            var total = tvSum.AddChild("File Total Amount ($) - " + _ifd.TotalFileAmount);
            total.AddChild("File Creation Date - " + _ifd.FileCreationDate);
            total.AddChild("Total No of Records - " + _ifd.TotalNumberofRecords);
            total.AddChild("Total No of Cash Letter - " + _objCashLetterSummary.Count);
            var cl = tvSum.AddChild("Total Cash Letter Amount ($) - " + _objCashLetterSummary.TotalCashLetterAmount);
            foreach (var icd in _objCashLetterSummary)
            {
                cl.AddChild("Cash Letter " + icd.CashLetterPosition + " Amount ($) - " + icd.CashLetterAmount);
            }
            if (_objCreditSummary != null && _objCreditSummary.Count > 0)
            {
                cl.AddChild("Total No of Credit Records - " + _objCreditSummary.Count);
                var credit = tvSum.AddChild("Total Credit Amount ($) - " + _objCreditSummary.TotalCreditRecordAmount);
                foreach (ICLCreditDetail crd in _objCreditSummary)
                {
                    credit.AddChild($"Posting Bank On-Us {crd.PostingAccBankOnUs.Trim()} Amount ($)- {crd.CreditAmount}");
                }
            }

            foreach (var node in tvSum)
            {
                var indent = CreateIndent(node.Level);
                Console.WriteLine($"{indent}{node.Data}");
            }
        }

        string CreateIndent(int depth)
        {
            depth += 1;
            var sb = new StringBuilder();
            for (var i = 0; i < depth * 2; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }

//  private void trvSummary_DrawNode(object sender, DrawTreeNodeEventArgs e)
//  {
//    e.DrawDefault = true;
//    if (e.Node.Text.Contains("Posting bank"))
//    {
//      e.DrawDefault = false;
//      string pbac = "Posting Bank On-Us ";
//      string amt = "Amount ($) - ";
//      string[] texts = e.Node.Text.Split(',');
//      TextRenderer.DrawText(e.Graphics, pbac, this.Font, e.Bounds, Color.Black, Color.Empty, TextFormatFlags.VerticalCenter);
//      using (Font font = new Font(this.Font, FontStyle.Regular))
//      {
//        SizeF s = e.Graphics.MeasureString(pbac, font);
//        TextRenderer.DrawText(e.Graphics, texts[1], font, new Point(e.Bounds.Left + (int)s.Width, e.Bounds.Top + 10), Color.Red, Color.Empty, TextFormatFlags.VerticalCenter);
//        s = e.Graphics.MeasureString(pbac + texts[1], font);
//        TextRenderer.DrawText(e.Graphics, amt, this.Font, new Point(e.Bounds.Left + (int)s.Width, e.Bounds.Top + 10), Color.Black, Color.Empty, TextFormatFlags.VerticalCenter);
//        s = e.Graphics.MeasureString(pbac + texts[1] + amt, font);
//        TextRenderer.DrawText(e.Graphics, texts[2], font, new Point(e.Bounds.Left + (int)s.Width, e.Bounds.Top + 10), Color.Blue, Color.Empty, TextFormatFlags.VerticalCenter);
//      }
//    }
//  }
    }

    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {

        public T Data { get; set; }
        public TreeNode<T> Parent { get; set; }
        public ICollection<TreeNode<T>> Children { get; set; }

        public bool IsRoot => Parent == null;

        public bool IsLeaf => Children.Count == 0;

        public int Level
        {
            get
            {
                if (IsRoot)
                    return 0;
                return Parent.Level + 1;
            }
        }

        public TreeNode(T data)
        {
            Data = data;
            Children = new LinkedList<TreeNode<T>>();
            ElementsIndex = new LinkedList<TreeNode<T>>();
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

        private ICollection<TreeNode<T>> ElementsIndex { get; set; }

        private void RegisterChildForSearch(TreeNode<T> node)
        {
            ElementsIndex.Add(node);
            Parent?.RegisterChildForSearch(node);
        }

        public TreeNode<T> FindTreeNode(Func<TreeNode<T>, bool> predicate)
        {
            return ElementsIndex.FirstOrDefault(predicate);
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
