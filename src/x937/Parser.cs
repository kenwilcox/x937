using System;
using System.IO;
using System.Text;

namespace x937
{
    public static class Parser
    {
        public static X9Recs ParseX9File(string x9File)
        {
            var ret = new X9Recs();

            var fs = new FileStream(x9File, FileMode.Open, FileAccess.Read, FileShare.Read);
            var curPos = 0;
            // 37 is EBCDIC encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // please???
            var br = new BinaryReader(fs, Encoding.GetEncoding(37));

            // Read first record
            var reclenB = br.ReadBytes(4); // to hold rec length in Big Endian byte order (motorola format) why Big Endian??
            curPos += 4;
            Array.Reverse(reclenB); // this is 'cause the rec length is in Big Endian order why? (probably some wise ass)
            var reclen = BitConverter.ToInt32(reclenB, 0); // convert rec length to integer

            // counts
            var fileRecCount = 0;
            // Flags for start and end of sections
            var fileStarted = false;
            var clStarted = false;
            var bundleStarted = false;
            var fileEnded = false;
            var clEnded = false;
            var bundleEnded = false;
            var checkStarted = false;
            var checkFront50 = false;
            var checkFront52 = false;
            var checkBack50 = false;
            var checkBack52 = false;

            // Loop thru the file
            while (reclen > 0 && !fileEnded)
            {
                //recB = new byte[reclen + 1];
                var recB = br.ReadBytes(reclen);
                curPos += reclen;
                if (fileRecCount % 100 == 0)
                {
                    // Do I care about displaying a progress?
                    //Console.WriteLine((readSize / 1024.0).ToString("###,###,###,###") + " KB of " + (fileSize / 1024.0).ToString("###,###,###,###") + " KB");
                }
                var rec = Encoding.ASCII.GetString(recB);
                fileRecCount += 1;
                switch (rec.Substring(0, 2))
                {
                    case "01": // File Header record
                        fileStarted = true;
                        ret.Add(new X9Rec("01", rec, ""));
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
                        var refKeyLen = int.Parse(rec.Substring(101));
                        // read image ref key and digital sig length
                        rec = Encoding.ASCII.GetString(recB, 0, 105 + refKeyLen + 5);
                        var sigLen = int.Parse(rec.Substring(105 + refKeyLen));
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
                            checkBack50 = false;
                            checkBack52 = false;
                            checkFront50 = false;
                            checkFront52 = false;
                        }
                        break;
                    case "70":
                        bundleEnded = true;
                        ret.Add(new X9Rec("70", rec, ""));
                        break;
                    case "90":
                        clEnded = true;
                        ret.Add(new X9Rec("90", rec, ""));
                        break;
                    case "99":
                        fileEnded = true;
                        ret.Add(new X9Rec("99", rec, ""));
                        break;
                    default: throw new NotImplementedException($"No handler for fields of type {rec.Substring(0, 2)} found");
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
    }
}
