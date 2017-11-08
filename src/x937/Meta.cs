﻿using System;
using System.Collections.Generic;
using System.Text;

namespace x937
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

        public static Meta GetMeta()
        {
            var meta = new Meta
            {
                { new Record("FileHeaderRecord", "01"), BuildT01Fields() },
                { new Record("CashLetterHeaderRecord", "10"), BuildT10Fields() },
                { new Record("BatchHeaderRecord", "20"), BuildT20Fields() },
                { new Record("CheckDetailRecord", "25"), BuildT25Fields() },
            };

            return meta;
        }

        public static X9Record GetObjectFor(Record record)
        {
            X9Record ret;
            switch (record.TypeId)
            {
                case "01": ret = new R01(); break;
                case "10": ret = new R10(); break;
                case "20": ret = new R20(); break;
                case "25": ret = new R25(); break;
                default: ret = new Unknown(); break;
            }
            return ret;
        }

        public static string GetTestStringFor(Record record)
        {
            var meta = GetMeta()[record];
            var sb = new StringBuilder();
            foreach (var field in meta)
            {
                switch (field.ValueType)
                {
                    case ValueType.Literal: sb.Append(field.Value);break;
                    case ValueType.RoutePattern: sb.Append("TTTTAAAAC");break;
                    case ValueType.Position: sb.Append("4242");break;
                    case ValueType.Date: sb.Append(field.Size == 8 ? "YYYYMMDD" : "HHmm");break;
                    case ValueType.Logical: sb.Append(GetLogical(field.Value, field.Size)); break;
                    case ValueType.Cr61: sb.Append("CR61");break; // CR61 is len(4)
                    case ValueType.Blank: sb.Append(GetBlank(field.Size)); break;
                    case ValueType.Undefined: sb.Append(GetUndefined(field.Type, field.Size)); break;
                    default: throw new Exception($"No processor for {field.ValueType}");
                }
            }

            return sb.ToString();
        }

        public static string GetClassFor(Record record)
        {
            var meta = GetMeta()[record];
            var sb = new StringBuilder();
            var props = new StringBuilder();
            sb.Append($"public class R{record.TypeId}: X9Record\n{{\n    public override void SetData(string data)\n    {{\n        base.SetData(data);\n");
            sb.Append($"        Debug.WriteLine(\"R{record.TypeId} SetData() called\");\n");
            foreach (var field in meta)
            {
                if (field.FieldName == "RecordType") continue; // part of base class, everyone has this
                props.Append($"    public string {field.FieldName} {{ get; set; }}\n");
                sb.Append($"        {field.FieldName} = Data.Substring({field.Position.Start}, {field.Size});\n");
            }
            sb.Append("    }\n\n");
            sb.Append(props);
            sb.Append("}");
            return sb.ToString();
        }

        private static string GetLogical(string value, int size)
        {
            var parts = value.Split('|');
            var idx = Rnd.Next(parts.Length);
            return parts[idx].PadLeft(size, ' ');
        }

        private static string GetBlank(int size)
        {
            return "".PadLeft(size, ' ');
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
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "01", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "StandardLevel", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "03", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "TestFileIndicator", Usage = "M", DocPosition = new Range(5, 5), Type = "A", Value = "T|P", ValueType = ValueType.Logical},
                new Field {Order = 4, FieldName = "ImmediateDestinationRoutingNumber", Usage = "M", DocPosition = new Range(6, 14), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 5, FieldName = "ImmediateOriginRoutingNumber", Usage = "M", DocPosition = new Range(15, 23), Type = "N", Value = "TTTTAAAACC", ValueType = ValueType.RoutePattern},
                new Field {Order = 6, FieldName = "FileCreationDate", Usage = "M", DocPosition = new Range(24, 31), Type = "N", Value = "YYMMDD", ValueType = ValueType.Date},
                new Field {Order = 7, FieldName = "FileCreationTime", Usage = "M", DocPosition = new Range(32, 35), Type = "N", Value = "HHmm", ValueType = ValueType.Date},
                new Field {Order = 8, FieldName = "ResendIndicator", Usage = "M", DocPosition = new Range(36, 36), Type = "A", Value = "N", ValueType = ValueType.Literal},
                new Field {Order = 9, FieldName = "ImmediateDestinationName", Usage = "M", DocPosition = new Range(37, 54), Type = "A", Value = "", ValueType = ValueType.Undefined},
                new Field {Order = 10, FieldName = "ImmediateOriginName", Usage = "C", DocPosition = new Range(55, 72), Type = "A", Value = "", ValueType = ValueType.Undefined},
                // I'm not sure how I'm going to handle field 11 yet...
                // Value = A normally, however if fields 4,5,6, and 7 are the same values on multiple files, then this would have a unique alpha or numeric
                //          character to make this file unique from other files, i.e., "B", "C", "0", "1", etc.
                new Field {Order = 11, FieldName = "FileIdModifier", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "A", ValueType = ValueType.Undefined},
                new Field {Order = 12, FieldName = "CountryCode", Usage = "C", DocPosition = new Range(74, 75), Type = "A", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 13, FieldName = "UserField", Usage = "C", DocPosition = new Range(76, 79), Type = "ANS", Value = "", ValueType = ValueType.Cr61},
                new Field {Order = 14, FieldName = "Reserved", Usage = "M", DocPosition = new Range(80, 80), Type = "B", Value = "", ValueType = ValueType.Blank}
            };
            return fields;
        }

        private static List<Field> BuildT10Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "10", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "CollectionTypeIndicator", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "01", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "DestinationRoutingNumber", Usage = "M", DocPosition = new Range(5, 13), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 4, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(14, 22), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 5, FieldName = "CashLetterBusinessDate", Usage = "M", DocPosition = new Range(23, 30), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 6, FieldName = "CashLetterCreationDate", Usage = "M", DocPosition = new Range(31, 38), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 7, FieldName = "CashLetterCreationTime", Usage = "M", DocPosition = new Range(39, 42), Type = "N", Value = "HHmm", ValueType = ValueType.Date},
                new Field {Order = 8, FieldName = "CashLetterRecordTypeIndicator", Usage = "M", DocPosition = new Range(43, 43), Type = "A", Value = "I", ValueType = ValueType.Literal},
                new Field {Order = 9, FieldName = "CashLetterDocumentationTypeIndicator", Usage = "C", DocPosition = new Range(44, 44), Type = "AN", Value = "G", ValueType = ValueType.Literal},
                // The Cash Letter ID must be a unique number within a Cash Letter Business Date.
                new Field {Order = 10, FieldName = "CashLetterId", Usage = "C", DocPosition = new Range(45, 52), Type = "AN", Value = "", ValueType = ValueType.Undefined},
                new Field {Order = 11, FieldName = "OriginatorContactName", Usage = "C", DocPosition = new Range(53, 66), Type = "ANS", Value = "", ValueType = ValueType.Undefined},
                new Field {Order = 12, FieldName = "OriginatorContactPhoneNumber", Usage = "C", DocPosition = new Range(67, 76), Type = "N", Value = "", ValueType = ValueType.Undefined},
                new Field {Order = 13, FieldName = "FedWorkType", Usage = "C", DocPosition = new Range(77, 77), Type = "AN", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 14, FieldName = "UserField", Usage = "C", DocPosition = new Range(78, 79), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 15, FieldName = "User", Usage = "O", DocPosition = new Range(80, 80), Type = "B", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT20Fields()
        {
            var field = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "20", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "CollectionTypeIndicator", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "01", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "DestinationRoutingNumber", Usage = "M", DocPosition = new Range(5, 13), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 4, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(14, 22), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 5, FieldName = "BatchBusinessDate", Usage = "M", DocPosition = new Range(23, 30), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 6, FieldName = "BatchCreationDate", Usage = "M", DocPosition = new Range(31, 38), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 7, FieldName = "BatchId", Usage = "M", DocPosition = new Range(39, 48), Type = "AN", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 8, FieldName = "BatchSequenceNumber", Usage = "M", DocPosition = new Range(49, 52), Type = "NB", Value = "0001", ValueType = ValueType.Position},
                new Field {Order = 9, FieldName = "CycleNumber", Usage = "C", DocPosition = new Range(53, 54), Type = "AN", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 10, FieldName = "ReturnLocationRoutingNumber", Usage = "C", DocPosition = new Range(55, 63), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 11, FieldName = "UserField", Usage = "M", DocPosition = new Range(64, 68), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 12, FieldName = "Reserved", Usage = "M", DocPosition = new Range(69, 80), Type = "B", Value = "10", ValueType = ValueType.Blank},
            };
            return field;
        }

        private static List<Field> BuildT25Fields()
        {
            var field = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "20", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "AuxiliaryOnUs", Usage = "C", DocPosition = new Range(3, 17), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
                new Field {Order = 3, FieldName = "ExternamProcessingCode", Usage = "C", DocPosition = new Range(18, 18), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
                // Everywhere else this is TTTTAAAAC, but here the C is specified....
                new Field {Order = 4, FieldName = "PayorBankRoutingNumber", Usage = "M", DocPosition = new Range(19, 26), Type = "N", Value = "TTTTAAAA", ValueType = ValueType.RoutePattern},
                new Field {Order = 5, FieldName = "PriorBankRoutingNumberCheckDigit", Usage = "M", DocPosition = new Range(27, 27), Type = "N", Value = "C", ValueType = ValueType.Literal},
                new Field {Order = 6, FieldName = "OnUs", Usage = "M", DocPosition = new Range(28, 47), Type = "NBSMOS", Value = "", ValueType = ValueType.NBSMOS},
                new Field {Order = 7, FieldName = "ItemAmount", Usage = "M", DocPosition = new Range(48, 57), Type = "N", Value = "C", ValueType = ValueType.LeadingZeros},
                new Field {Order = 8, FieldName = "ECEInstitutionItemSequenceNumber", Usage = "M", DocPosition = new Range(58, 72), Type = "NB", Value = "", ValueType = ValueType.Sequence},
                new Field {Order = 9, FieldName = "DocumentationTypeIndicator", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "G", ValueType = ValueType.Literal},
                new Field {Order = 10, FieldName = "ReturnAcceptanceIndicator", Usage = "C", DocPosition = new Range(74, 74), Type = "AN", Value = "6", ValueType = ValueType.Literal},
                new Field {Order = 11, FieldName = "MICRValidIndicator", Usage = "C", DocPosition = new Range(75, 75), Type = "N", Value = "1|2|3|4", ValueType = ValueType.Logical},
                new Field {Order = 12, FieldName = "BOFDIndicator", Usage = "M", DocPosition = new Range(76, 76), Type = "A", Value = "Y|N|U", ValueType = ValueType.Logical},
                new Field {Order = 13, FieldName = "CheckDetailRecordAddendumCount", Usage = "M", DocPosition = new Range(77, 78), Type = "N", Value = "1", ValueType = ValueType.Literal},
                new Field {Order = 14, FieldName = "CorrectionIndicator", Usage = "M", DocPosition = new Range(79, 79), Type = "N", Value = "0|1|2|3|4", ValueType = ValueType.Logical},
                new Field {Order = 15, FieldName = "ArchiveTypeIndicator", Usage = "C", DocPosition = new Range(80, 80), Type = "AN", Value = "", ValueType = ValueType.Blank},
            };
            return field;
        }
    }

    public class Meta : Dictionary<Record, List<Field>>
    {
    }

    public class Record: IEquatable<Record>
    {
        public readonly string Name;// { get; set; }
        //public string Name { get; private set; }
        public readonly string TypeId;// { get; set; }
        //public string TypeId { get; private set; }

        public Record(string name, string typeId)
        {
            Name = name;
            TypeId = typeId;
        }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode() * Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Record);
        }

        public bool Equals(Record obj)
        {
            return obj != null && obj.Name == Name && obj.TypeId == TypeId;
        }
    }

    public class Field
    {
        public int Order { get; set; }
        public string FieldName { get; set; }
        public string Usage { get; set; }
        public Range DocPosition { private get; set; }
        //public int Size { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public ValueType ValueType { get; set; }
        // Because the documentation is 0 based and .net is 0 based...
        public Range Position
        {
            get
            {
                return new Range { Start = DocPosition.Start -1, End = DocPosition.End};
            }
        }

        public int Size
        {
            get
            {
                return Position.End - Position.Start;
            }
        }
    }

    public struct Range
    {
        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start;
        public int End;
    }

    public enum ValueType
    {
        Undefined,
        Literal,
        Logical,
        Date,
        RoutePattern,
        Blank,
        Cr61,
        Position,
        NBSM,
        NBSMOS,
        LeadingZeros,
        Sequence,
    }

    public static class MetaExt
    {
        public static Range To(this int start, int end)
        {
            return new Range {Start = start, End = end};
        }

        public static string Substring(this string obj, Range range)
        {
            if (range.End < 0) range.End = obj.Length + range.End;
            return obj.Substring(range.Start, range.End - range.Start);
        }
    }
}
