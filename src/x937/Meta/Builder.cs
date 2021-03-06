﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x937.Meta
{
    public static class Builder
    {
        private static readonly Random Rnd = new Random();
        private static readonly Dictionary<string, string> FieldType;
        static Builder()
        {
            //const string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //const string alphaNum = ;
            FieldType = new Dictionary<string, string>
            {
                {"A", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
                {"N", "1234567890" },
                {"B", "          " },
                {"S", "----------" },
                {"AN", "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6" },
                {"ANS", "6Z-5Y-4X-3W-2V-1U-0T-9S-8R-7Q-6P-5O-4N-3M-2L-1K-0J-9I-8H-7G-6F-5E-4D-3C-2B-1A" },
                {"NB", "1 2 3 4 5 6 7 8 9 0 " },
                {"NS", "1-2-3-4-5-6-7-8-9-0-"},
                // I'm not sure about these yet...
                //{"Binary", "" },
                //{ "NBSM", ""},
                //{ "NBSMOS", ""}
            };
        }

        public static Metadata GetMeta()
        {
            var meta = new Metadata
            {
                {new XRecord("FileHeaderRecord", "01"), BuildT01Fields()},
                {new XRecord("CashLetterHeaderRecord", "10"), BuildT10Fields()},
                {new XRecord("BatchHeaderRecord", "20"), BuildT20Fields()},
                {new XRecord("CheckDetailRecord", "25"), BuildT25Fields()},
                {new XRecord("CheckDetailAddendumARecord", "26"), BuildT26Fields()},
                {new XRecord("ImageViewDetailRecord", "50"), BuildT50Fields()},
                {new XRecord("ImageViewDataRecord", "52"), BuildT52Fields()},
                {new XRecord("CreditDetailRecord", "61"), BuildT61Fields()},
                {new XRecord("BatchControlRecord", "70"), BuildT70Fields()},
                {new XRecord("CashLetterControlRecord", "90"), BuildT90Fields()},
                {new XRecord("FileControlRecord", "99"), BuildT99Fields()}
            };

            return meta;
        }

        public static X9Record GetObjectFor(XRecord record)
        {
            X9Record ret;
            switch (record.TypeId)
            {
                case "01": ret = new R01(); break;
                case "10": ret = new R10(); break;
                case "20": ret = new R20(); break;
                case "25": ret = new R25(); break;
                case "26": ret = new R26(); break;
                case "50": ret = new R50(); break;
                case "52": ret = new R52(); break;
                case "61": ret = new R61(); break;
                case "70": ret = new R70(); break;
                case "90": ret = new R90(); break;
                case "99": ret = new R99(); break;
                default: ret = new Unknown(); break;
            }
            return ret;
        }

        public static string GetTestStringFor(IEnumerable<Field> meta)
        {
            var sb = new StringBuilder();
            var previousType = DataType.Literal;
            var binary = string.Empty;
            foreach (var field in meta)
            {
                var length = sb.Length;
                switch (field.DataType)
                {
                    case DataType.Literal: sb.Append(field.Value); break;
                    case DataType.RoutePattern: sb.Append("TTTTAAAAC".Substring(0, field.Size)); break; // some route patterns exclude the check digit
                    case DataType.Position: sb.Append("4242"); break;
                    case DataType.Date: sb.Append(field.Size == 8 ? "YYYYMMDD" : "HHmm"); break;
                    case DataType.Logical: sb.Append(GetLogical(field.Value, field.Size)); break;
                    case DataType.Cr61: sb.Append("CR61"); break; // CR61 is len(4)
                    case DataType.Blank: sb.Append(GetBlank(field.Size, field.Value)); break;
                    case DataType.Undefined: sb.Append(GetUndefined(field.Type, field.Size)); break;
                    case DataType.NBSM: sb.Append(GetRepeating('x', field.Size)); break;
                    case DataType.NBSMOS: sb.Append(GetRepeating('z', field.Size)); break;
                    case DataType.LeadingZeros: sb.Append(GetUndefined(field.Type, field.Size)); break;
                    case DataType.Sequence: sb.Append(GetUndefined(field.Type, field.Size)); break;
                    case DataType.Length:
                        binary = GetRandomData(Rnd.Next(2048, 65536)); // 2 - 64KB
                        sb.Append(binary.Length.ToString().PadLeft(field.Size));
                        break;
                    case DataType.Binary:
                        if (previousType == DataType.Length) sb.Append(binary);
                        else throw new InvalidOperationException("DataType Binary used before ValueType Length");
                        break;
                    default: throw new NotImplementedException($"No processor for {field.DataType}");
                }
                // I believe this is no longer needed...
                if (field.DataType != DataType.Binary)
                {
                    if (sb.Length != length + field.Size) throw new ArgumentOutOfRangeException($"Field {field.FieldName} generated a size of {sb.Length - length}, but it should have been {field.Size}");
                }
                else
                {
                    if (sb.Length != length + binary.Length) throw new ArgumentOutOfRangeException($"Field {field.FieldName} should have been {binary.Length}");
                }
                previousType = field.DataType;
            }

            return sb.ToString();
        }

        public static string GetRandomData(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-1234567890abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        public static string GetClassFor(XRecord record)
        {
            var meta = GetMeta()[record];
            var sb = new StringBuilder();
            var props = new StringBuilder();
            sb.Append($"public class R{record.TypeId}: X9Record\n{{\n    public override void SetData(string data, byte[] optional = null)\n    {{\n        base.SetData(data, optional);\n");
            sb.Append($"        Debug.WriteLine(\"R{record.TypeId} SetData() called\");\n");
            foreach (var field in meta)
            {
                if (field.FieldName == "RecordType") continue; // part of base class, everyone has this

                var dataType = "string";
                if (field.DataType == DataType.Binary) dataType = "byte[]";
                props.Append($"    public {dataType} {field.FieldName} {{ get; set; }}\n");
                if (field.DataType != DataType.Binary)
                {
                    sb.Append($"        {field.FieldName} = Data.Substring({field.Position.Start}, {field.Size});\n");
                }
                else
                {
                    //, Data.Length
                    sb.Append($"        {field.FieldName} = optional;\n");
                }
            }
            sb.Append("    }\n\n");
            sb.Append(props);
            sb.Append("}");
            return sb.ToString();
        }

        private static string GetLogical(string value, int size)
        {
            var parts = value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var idx = Rnd.Next(parts.Length);
            return parts[idx].PadLeft(size, ' ');
        }

        private static string GetBlank(int size, string repeatingChar = "")
        {
            var repeat = '-'; // Dashes are easier to see
            if (repeatingChar != string.Empty) repeat = repeatingChar[0];
            return "".PadLeft(size, repeat);
        }

        private static string GetRepeating(char character, int size)
        {
            return "".PadLeft(size, character);
        }

        private static string GetUndefined(string type, int size)
        {
            var ret = GetBlank(size);
            if (!FieldType.ContainsKey(type)) return ret.Substring(0, size);
            ret = FieldType[type];
            while (ret.Length < size) ret += ret;
            return ret.Substring(0, size);
        }

        private static List<Field> BuildT01Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "01", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "StandardLevel", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "03", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "TestFileIndicator", Usage = "M", DocPosition = new Range(5, 5), Type = "A", Value = "T|P", DataType = DataType.Logical},
                new Field {Order = 4, FieldName = "ImmediateDestinationRoutingNumber", Usage = "M", DocPosition = new Range(6, 14), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 5, FieldName = "ImmediateOriginRoutingNumber", Usage = "M", DocPosition = new Range(15, 23), Type = "N", Value = "TTTTAAAACC", DataType = DataType.RoutePattern},
                new Field {Order = 6, FieldName = "FileCreationDate", Usage = "M", DocPosition = new Range(24, 31), Type = "N", Value = "YYMMDD", DataType = DataType.Date},
                new Field {Order = 7, FieldName = "FileCreationTime", Usage = "M", DocPosition = new Range(32, 35), Type = "N", Value = "HHmm", DataType = DataType.Date},
                new Field {Order = 8, FieldName = "ResendIndicator", Usage = "M", DocPosition = new Range(36, 36), Type = "A", Value = "N", DataType = DataType.Literal},
                new Field {Order = 9, FieldName = "ImmediateDestinationName", Usage = "M", DocPosition = new Range(37, 54), Type = "A", Value = "", DataType = DataType.Undefined},
                new Field {Order = 10, FieldName = "ImmediateOriginName", Usage = "C", DocPosition = new Range(55, 72), Type = "A", Value = "", DataType = DataType.Undefined},
                // I'm not sure how I'm going to handle field 11 yet...
                // Value = A normally, however if fields 4,5,6, and 7 are the same values on multiple files, then this would have a unique alpha or numeric
                //          character to make this file unique from other files, i.e., "B", "C", "0", "1", etc.
                new Field {Order = 11, FieldName = "FileIdModifier", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "A", DataType = DataType.Undefined},
                new Field {Order = 12, FieldName = "CountryCode", Usage = "C", DocPosition = new Range(74, 75), Type = "A", Value = "", DataType = DataType.Blank},
                new Field {Order = 13, FieldName = "UserField", Usage = "C", DocPosition = new Range(76, 79), Type = "ANS", Value = "", DataType = DataType.Cr61},
                new Field {Order = 14, FieldName = "Reserved", Usage = "M", DocPosition = new Range(80, 80), Type = "B", Value = "", DataType = DataType.Blank}
            };
            return fields;
        }

        private static List<Field> BuildT10Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "10", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "CollectionTypeIndicator", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "01", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "DestinationRoutingNumber", Usage = "M", DocPosition = new Range(5, 13), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 4, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(14, 22), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 5, FieldName = "CashLetterBusinessDate", Usage = "M", DocPosition = new Range(23, 30), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 6, FieldName = "CashLetterCreationDate", Usage = "M", DocPosition = new Range(31, 38), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 7, FieldName = "CashLetterCreationTime", Usage = "M", DocPosition = new Range(39, 42), Type = "N", Value = "HHmm", DataType = DataType.Date},
                new Field {Order = 8, FieldName = "CashLetterRecordTypeIndicator", Usage = "M", DocPosition = new Range(43, 43), Type = "A", Value = "I", DataType = DataType.Literal},
                new Field {Order = 9, FieldName = "CashLetterDocumentationTypeIndicator", Usage = "C", DocPosition = new Range(44, 44), Type = "AN", Value = "G", DataType = DataType.Literal},
                // The Cash Letter ID must be a unique number within a Cash Letter Business Date.
                new Field {Order = 10, FieldName = "CashLetterId", Usage = "C", DocPosition = new Range(45, 52), Type = "AN", Value = "", DataType = DataType.Undefined},
                new Field {Order = 11, FieldName = "OriginatorContactName", Usage = "C", DocPosition = new Range(53, 66), Type = "ANS", Value = "", DataType = DataType.Undefined},
                new Field {Order = 12, FieldName = "OriginatorContactPhoneNumber", Usage = "C", DocPosition = new Range(67, 76), Type = "N", Value = "", DataType = DataType.Undefined},
                new Field {Order = 13, FieldName = "FedWorkType", Usage = "C", DocPosition = new Range(77, 77), Type = "AN", Value = "", DataType = DataType.Blank},
                new Field {Order = 14, FieldName = "UserField", Usage = "C", DocPosition = new Range(78, 79), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 15, FieldName = "User", Usage = "O", DocPosition = new Range(80, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT20Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "20", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "CollectionTypeIndicator", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "01", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "DestinationRoutingNumber", Usage = "M", DocPosition = new Range(5, 13), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 4, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(14, 22), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 5, FieldName = "BatchBusinessDate", Usage = "M", DocPosition = new Range(23, 30), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 6, FieldName = "BatchCreationDate", Usage = "M", DocPosition = new Range(31, 38), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 7, FieldName = "BatchId", Usage = "M", DocPosition = new Range(39, 48), Type = "AN", Value = "", DataType = DataType.Blank},
                new Field {Order = 8, FieldName = "BatchSequenceNumber", Usage = "M", DocPosition = new Range(49, 52), Type = "NB", Value = "0001", DataType = DataType.Position},
                new Field {Order = 9, FieldName = "CycleNumber", Usage = "C", DocPosition = new Range(53, 54), Type = "AN", Value = "", DataType = DataType.Blank},
                new Field {Order = 10, FieldName = "ReturnLocationRoutingNumber", Usage = "C", DocPosition = new Range(55, 63), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 11, FieldName = "UserField", Usage = "M", DocPosition = new Range(64, 68), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 12, FieldName = "Reserved", Usage = "M", DocPosition = new Range(69, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT25Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "25", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "AuxiliaryOnUs", Usage = "C", DocPosition = new Range(3, 17), Type = "NBSM", Value = "", DataType = DataType.NBSM},
                new Field {Order = 3, FieldName = "ExternalProcessingCode", Usage = "C", DocPosition = new Range(18, 18), Type = "NBSM", Value = "", DataType = DataType.NBSM},
                // Everywhere else this is TTTTAAAAC, but here the C is specified....
                new Field {Order = 4, FieldName = "PayorBankRoutingNumber", Usage = "M", DocPosition = new Range(19, 26), Type = "N", Value = "TTTTAAAA", DataType = DataType.RoutePattern},
                new Field {Order = 5, FieldName = "PriorBankRoutingNumberCheckDigit", Usage = "M", DocPosition = new Range(27, 27), Type = "N", Value = "C", DataType = DataType.Literal},
                new Field {Order = 6, FieldName = "OnUs", Usage = "M", DocPosition = new Range(28, 47), Type = "NBSMOS", Value = "", DataType = DataType.NBSMOS},
                new Field {Order = 7, FieldName = "ItemAmount", Usage = "M", DocPosition = new Range(48, 57), Type = "N", Value = "C", DataType = DataType.LeadingZeros},
                new Field {Order = 8, FieldName = "ECEInstitutionItemSequenceNumber", Usage = "M", DocPosition = new Range(58, 72), Type = "NB", Value = "", DataType = DataType.Sequence},
                new Field {Order = 9, FieldName = "DocumentationTypeIndicator", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "G", DataType = DataType.Literal},
                new Field {Order = 10, FieldName = "ReturnAcceptanceIndicator", Usage = "C", DocPosition = new Range(74, 74), Type = "AN", Value = "6", DataType = DataType.Literal},
                new Field {Order = 11, FieldName = "MICRValidIndicator", Usage = "C", DocPosition = new Range(75, 75), Type = "N", Value = "1|2|3|4", DataType = DataType.Logical},
                new Field {Order = 12, FieldName = "BOFDIndicator", Usage = "M", DocPosition = new Range(76, 76), Type = "A", Value = "Y|N|U", DataType = DataType.Logical},
                new Field {Order = 13, FieldName = "CheckDetailRecordAddendumCount", Usage = "M", DocPosition = new Range(77, 78), Type = "N", Value = "12", DataType = DataType.Literal},
                new Field {Order = 14, FieldName = "CorrectionIndicator", Usage = "M", DocPosition = new Range(79, 79), Type = "N", Value = "0|1|2|3|4", DataType = DataType.Logical},
                new Field {Order = 15, FieldName = "ArchiveTypeIndicator", Usage = "C", DocPosition = new Range(80, 80), Type = "AN", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT26Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "26", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "CheckDetailAddendumARecordNumber", Usage = "M", DocPosition = new Range(3, 3), Type = "N", Value = "1", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "BOFDRoutingNumber", Usage = "C", DocPosition = new Range(4, 12), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 4, FieldName = "BOFDBusinessDate", Usage = "C", DocPosition = new Range(13, 20), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 5, FieldName = "BOFDItemSequenceNumber", Usage = "C", DocPosition = new Range(21, 35), Type = "NB", Value = "", DataType = DataType.Sequence},
                new Field {Order = 6, FieldName = "BOFDDepositAccountNumber", Usage = "C", DocPosition = new Range(36, 53), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 7, FieldName = "BOFDDepositBranch", Usage = "C", DocPosition = new Range(54, 58), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 8, FieldName = "PayeeName", Usage = "C", DocPosition = new Range(59, 73), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 9, FieldName = "TruncationIndicator", Usage = "C", DocPosition = new Range(74, 74), Type = "A", Value = "Y", DataType = DataType.Literal},
                new Field {Order = 10, FieldName = "BOFDConversionIndicator", Usage = "C", DocPosition = new Range(75, 75), Type = "AN", Value = "2", DataType = DataType.Literal},
                new Field {Order = 11, FieldName = "BOFDCorrectionIndicator", Usage = "C", DocPosition = new Range(76, 76), Type = "N", Value = "0", DataType = DataType.Literal},
                new Field {Order = 12, FieldName = "UserField", Usage = "C", DocPosition = new Range(77, 77), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 13, FieldName = "Reserved", Usage = "M", DocPosition = new Range(78, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT50Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "50", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "ImageIndicator", Usage = "M", DocPosition = new Range(3, 3), Type = "N", Value = "1", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "ImageCreatorRoutingNumber", Usage = "M", DocPosition = new Range(4, 12), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 4, FieldName = "ImageCreatorDate", Usage = "M", DocPosition = new Range(13, 20), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 5, FieldName = "ImageViewFormatIndicator", Usage = "M", DocPosition = new Range(21, 22), Type = "N", Value = "0 ", DataType = DataType.Literal},
                new Field {Order = 6, FieldName = "ImageViewCompressionAlgorithmIdentifier", Usage = "M", DocPosition = new Range(23, 24), Type = "N", Value = "0 ", DataType = DataType.Literal},
                new Field {Order = 7, FieldName = "ImageViewDataSize", Usage = "C", DocPosition = new Range(25, 31), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 8, FieldName = "ViewSideIndicator", Usage = "M", DocPosition = new Range(32, 32), Type = "N", Value = "0|1", DataType = DataType.Logical},
                new Field {Order = 9, FieldName = "ViewDescriptor", Usage = "M", DocPosition = new Range(33, 34), Type = "N", Value = "0 ", DataType = DataType.Literal},
                new Field {Order = 10, FieldName = "DigitalSignatureIndicator", Usage = "M", DocPosition = new Range(35, 35), Type = "NB", Value = "0", DataType = DataType.Literal},
                new Field {Order = 11, FieldName = "DigitalSignatureMethod", Usage = "C", DocPosition = new Range(36, 37), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 12, FieldName = "SecurityKeySize", Usage = "C", DocPosition = new Range(38, 42), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 13, FieldName = "StartOfProtectedData", Usage = "C", DocPosition = new Range(43, 49), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 14, FieldName = "LengthOfProtectedData", Usage = "C", DocPosition = new Range(50, 56), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 15, FieldName = "ImageRecreateIndicator", Usage = "C", DocPosition = new Range(57, 57), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 16, FieldName = "UserField", Usage = "C", DocPosition = new Range(58, 65), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 17, FieldName = "Reserved", Usage = "M", DocPosition = new Range(66, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT52Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "52", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(3, 11), Type = "N", Value = "TTTTAAAAC", DataType = DataType.RoutePattern},
                new Field {Order = 3, FieldName = "BatchBusinessDate", Usage = "M", DocPosition = new Range(12, 19), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 4, FieldName = "CycleNumber", Usage = "M", DocPosition = new Range(20, 21), Type = "AN", Value = "", DataType = DataType.Blank},
                new Field {Order = 5, FieldName = "ECEInstitutionItemSequenceNumber", Usage = "M", DocPosition = new Range(22, 36), Type = "NB", Value = "", DataType = DataType.Sequence},
                new Field {Order = 6, FieldName = "SecurityOriginatorName", Usage = "C", DocPosition = new Range(37, 52), Type = "ANS", Value = "x", DataType = DataType.Blank},
                new Field {Order = 7, FieldName = "SecurityAuthenticator", Usage = "C", DocPosition = new Range(53, 68), Type = "ANS", Value = "v", DataType = DataType.Blank},
                new Field {Order = 8, FieldName = "SecurityKeyName", Usage = "C", DocPosition = new Range(69, 84), Type = "ANS", Value = "*", DataType = DataType.Blank},

                new Field {Order = 9, FieldName = "ClippingOrigin", Usage = "M", DocPosition = new Range(85, 85), Type = "NB", Value = "0", DataType = DataType.Literal},
                new Field {Order = 10, FieldName = "ClippingCoordinateH1", Usage = "C", DocPosition = new Range(86, 89), Type = "N", Value = "#", DataType = DataType.Blank},
                new Field {Order = 11, FieldName = "ClippingCoordinateH2", Usage = "C", DocPosition = new Range(90, 93), Type = "N", Value = "^", DataType = DataType.Blank},
                new Field {Order = 12, FieldName = "ClippingCoordinateV1", Usage = "C", DocPosition = new Range(94, 97), Type = "N", Value = "x", DataType = DataType.Blank},
                new Field {Order = 13, FieldName = "ClippingCoordinateV2", Usage = "C", DocPosition = new Range(98, 101), Type = "N", Value = "", DataType = DataType.Blank},
                new Field {Order = 14, FieldName = "LengthOfImageReferenceKey", Usage = "M", DocPosition = new Range(102, 105), Type = "NB", Value = "0000", DataType = DataType.Literal},

                // I'm not sure at all about these yet....
                //new Field {Order = 15, FieldName = "ImageReferenceKey", Usage = "C", DocPosition = new Range(106, 110), Type = "N", Value = "52", DataType = DataType.Literal},
                new Field {Order = 15, FieldName = "LengthOfDigitalSignature", Usage = "M", DocPosition = new Range(106, 110), Type = "N", Value = "00000", DataType = DataType.Literal},
                //new Field {Order = 17, FieldName = "DigitalSignature", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "52", DataType = DataType.Literal},

                new Field {Order = 16, FieldName = "LengthOfImageData", Usage = "M", DocPosition = new Range(111, 117), Type = "N", Value = "", DataType = DataType.Length},
                // This is from start position to the end... what ever that may be
                new Field {Order = 17, FieldName = "ImageData", Usage = "M", DocPosition = new Range(118, Utils.EndOfString), Type = "Binary", Value = "", DataType = DataType.Binary},
            };
            return fields;
        }

        private static List<Field> BuildT61Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "61", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "AuxiliaryOnUs", Usage = "M", DocPosition = new Range(3, 17), Type = "NBSM", Value = "", DataType = DataType.NBSM},
                new Field {Order = 3, FieldName = "ExternalProcessingCode", Usage = "C", DocPosition = new Range(18, 18), Type = "ANS", Value = "", DataType = DataType.Blank},
                new Field {Order = 4, FieldName = "PayorBankRoutingNumber", Usage = "M", DocPosition = new Range(19, 27), Type = "N", Value = "522000410", DataType = DataType.Literal},
                new Field {Order = 5, FieldName = "CreditAccountNumberOnUs", Usage = "C", DocPosition = new Range(28, 47), Type = "NBSM", Value = "", DataType = DataType.NBSM},
                new Field {Order = 6, FieldName = "ItemAccount", Usage = "M", DocPosition = new Range(48, 57), Type = "N", Value = "9999999999", DataType = DataType.Literal},
                new Field {Order = 7, FieldName = "ECEInstitutionItemNumber", Usage = "M", DocPosition = new Range(58, 72), Type = "NB", Value = "", DataType = DataType.Sequence},
                new Field {Order = 8, FieldName = "DocumentationTypeIndicator", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "*", DataType = DataType.Blank},
                new Field {Order = 9, FieldName = "TypeOfAccountCode", Usage = "C", DocPosition = new Range(74, 74), Type = "AN", Value = "-", DataType = DataType.Blank},
                new Field {Order = 10, FieldName = "SourceOfWorkCode", Usage = "C", DocPosition = new Range(75, 75), Type = "AN", Value = "^", DataType = DataType.Blank},
                new Field {Order = 11, FieldName = "WorkType", Usage = "C", DocPosition = new Range(76, 76), Type = "AN", Value = "", DataType = DataType.Blank},
                new Field {Order = 12, FieldName = "DebitCreditIndicator", Usage = "C", DocPosition = new Range(77, 77), Type = "AN", Value = "#", DataType = DataType.Blank},
                new Field {Order = 13, FieldName = "Reserved", Usage = "M", DocPosition = new Range(78, 80), Type = "B", Value = "*", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT70Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "70", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "ItemsWithinBatchCount", Usage = "M", DocPosition = new Range(3, 6), Type = "N", Value = "0000", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "BatchTotalAmount", Usage = "M", DocPosition = new Range(7, 18), Type = "N", Value = "xxxxxxxxxxxx", DataType = DataType.Literal},
                new Field {Order = 4, FieldName = "MICRValidTotalAmount", Usage = "C", DocPosition = new Range(19, 30), Type = "N", Value = "9", DataType = DataType.Blank},
                new Field {Order = 5, FieldName = "ImagesWithinBatchCount", Usage = "C", DocPosition = new Range(31, 35), Type = "N", Value = "-", DataType = DataType.Blank},
                new Field {Order = 6, FieldName = "UserField", Usage = "C", DocPosition = new Range(36, 55), Type = "ANS", Value = "*", DataType = DataType.Blank},
                new Field {Order = 7, FieldName = "Reserved", Usage = "M", DocPosition = new Range(56, 80), Type = "B", Value = "#", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT90Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "90", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "BatchCount", Usage = "M", DocPosition = new Range(3, 8), Type = "N", Value = "999999", DataType = DataType.Literal},
                new Field {Order = 3, FieldName = "ItemsWithinCashLetterCount", Usage = "M", DocPosition = new Range(9, 16), Type = "N", Value = "0", DataType = DataType.Undefined},
                new Field {Order = 4, FieldName = "CashLetterTotalAmount", Usage = "M", DocPosition = new Range(17, 30), Type = "N", Value = "9", DataType = DataType.Undefined},
                // Docs state A, but it should be N, since it's a number
                new Field {Order = 5, FieldName = "ImagesWithinCashLetterCount", Usage = "C", DocPosition = new Range(31, 39), Type = "N", Value = "2", DataType = DataType.Undefined},
                new Field {Order = 6, FieldName = "CustomerName", Usage = "C", DocPosition = new Range(40, 57), Type = "A", Value = "x", DataType = DataType.Undefined},
                new Field {Order = 7, FieldName = "CreditDate", Usage = "C", DocPosition = new Range(58, 65), Type = "N", Value = "YYYYMMDD", DataType = DataType.Date},
                new Field {Order = 8, FieldName = "Reserved", Usage = "M", DocPosition = new Range(66, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT99Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "99", DataType = DataType.Literal},
                new Field {Order = 2, FieldName = "CashLetterCount", Usage = "M", DocPosition = new Range(3, 8), Type = "N", Value = "", DataType = DataType.Undefined},
                new Field {Order = 3, FieldName = "TotalRecordCount", Usage = "M", DocPosition = new Range(9, 16), Type = "N", Value = "", DataType = DataType.Undefined},
                new Field {Order = 4, FieldName = "TotalItemCount", Usage = "M", DocPosition = new Range(17, 24), Type = "N", Value = "", DataType = DataType.Undefined},
                new Field {Order = 5, FieldName = "FileTotalAmount", Usage = "M", DocPosition = new Range(25, 40), Type = "N", Value = "", DataType = DataType.Undefined},
                new Field {Order = 6, FieldName = "ImmediateOriginContactName", Usage = "C", DocPosition = new Range(41, 54), Type = "ANS", Value = "99", DataType = DataType.Undefined},
                new Field {Order = 7, FieldName = "ImmediateOriginContactPhoneNumber", Usage = "C", DocPosition = new Range(55, 64), Type = "N", Value = "99", DataType = DataType.Undefined},
                new Field {Order = 8, FieldName = "Reserved", Usage = "M", DocPosition = new Range(65, 80), Type = "B", Value = "", DataType = DataType.Blank},
            };
            return fields;
        }
    }
}
