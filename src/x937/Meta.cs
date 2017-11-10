using System;
using System.Collections.Generic;
using System.Linq;
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
                {new Record("FileHeaderRecord", "01"), BuildT01Fields()},
                {new Record("CashLetterHeaderRecord", "10"), BuildT10Fields()},
                {new Record("BatchHeaderRecord", "20"), BuildT20Fields()},
                {new Record("CheckDetailRecord", "25"), BuildT25Fields()},
                {new Record("CheckDetailAddendumARecord", "26"), BuildT26Fields()},
                {new Record("ImageViewDetailRecord", "50"), BuildT50Fields()},
                {new Record("ImageViewDataRecord", "52"), BuildT52Fields()},
                {new Record("CreditDetailRecord", "61"), BuildT61Fields() },
                {new Record("BatchControlRecord", "70"), BuildT70Fields() },
                {new Record("CashLetterControlRecord", "90"), BuildT90Fields() }
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
                case "26": ret = new R26(); break;
                case "50": ret = new R50(); break;
                case "52": ret = new R52(); break;
                case "61": ret = new R61(); break;
                case "70": ret = new R70(); break;
                case "90": ret = new R90(); break;
                default: ret = new Unknown(); break;
            }
            return ret;
        }

        public static string GetTestStringFor(Record record)
        {
            var meta = GetMeta()[record];
            var sb = new StringBuilder();
            var previousType = ValueType.Literal;
            var binary = string.Empty;
            foreach (var field in meta)
            {
                var length = sb.Length;
                switch (field.ValueType)
                {
                    case ValueType.Literal: sb.Append(field.Value);break;
                    case ValueType.RoutePattern: sb.Append("TTTTAAAAC".Substring(0, field.Size));break; // some route patterns exclude the check digit
                    case ValueType.Position: sb.Append("4242");break;
                    case ValueType.Date: sb.Append(field.Size == 8 ? "YYYYMMDD" : "HHmm");break;
                    case ValueType.Logical: sb.Append(GetLogical(field.Value, field.Size)); break;
                    case ValueType.Cr61: sb.Append("CR61");break; // CR61 is len(4)
                    case ValueType.Blank: sb.Append(GetBlank(field.Size, field.Value)); break;
                    case ValueType.Undefined: sb.Append(GetUndefined(field.Type, field.Size)); break;
                        // TODO update these with better test values
                    case ValueType.NBSM: sb.Append(GetRepeating('x', field.Size)); break;
                    case ValueType.NBSMOS: sb.Append(GetRepeating('z', field.Size));break;
                    case ValueType.LeadingZeros: sb.Append(GetUndefined(field.Type, field.Size)); break;
                    case ValueType.Sequence: sb.Append(GetUndefined(field.Type, field.Size));break;
                    case ValueType.Length:
                        binary = GetRandomData(Rnd.Next(2048, 65536)); // 2 - 64KB
                        sb.Append(binary.Length.ToString().PadLeft(field.Size));
                        break;
                    case ValueType.Binary:
                        if (previousType == ValueType.Length) sb.Append(binary);
                        else throw new Exception("ValueType Binary used before ValueType Length");
                        break;
                    default: throw new Exception($"No processor for {field.ValueType}");
                }
                if (field.ValueType != ValueType.Binary)
                {
                    if (sb.Length != length + field.Size) throw new Exception($"Field {field.FieldName} generated a size of {sb.Length - length}, but it should have been {field.Size}");
                }
                else
                {
                    if (sb.Length != length + binary.Length) throw new Exception($"Field {field.FieldName} should have been {binary.Length}");
                }
                previousType = field.ValueType;
            }

            return sb.ToString();
        }

        public static string GetRandomData(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-1234567890abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        public static string GetClassFor(Record record)
        {
            var meta = GetMeta()[record];
            var sb = new StringBuilder();
            var props = new StringBuilder();
            sb.Append($"public class R{record.TypeId}: X9Record\n{{\n    public override void SetData(string data, byte[] optional = null)\n    {{\n        base.SetData(data, optional);\n");
            sb.Append($"        Debug.WriteLine(\"R{record.TypeId} SetData() called\");\n");
            foreach (var field in meta)
            {
                if (field.FieldName == "RecordType") continue; // part of base class, everyone has this

                var fieldType = "string";
                if (field.ValueType == ValueType.Binary) fieldType = "byte[]";
                props.Append($"    public {fieldType} {field.FieldName} {{ get; set; }}\n");
                if (field.ValueType != ValueType.Binary)
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
            var parts = value.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
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
            var fields = new List<Field>
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
                new Field {Order = 12, FieldName = "Reserved", Usage = "M", DocPosition = new Range(69, 80), Type = "B", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT25Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "25", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "AuxiliaryOnUs", Usage = "C", DocPosition = new Range(3, 17), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
                new Field {Order = 3, FieldName = "ExternalProcessingCode", Usage = "C", DocPosition = new Range(18, 18), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
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
                new Field {Order = 13, FieldName = "CheckDetailRecordAddendumCount", Usage = "M", DocPosition = new Range(77, 78), Type = "N", Value = "12", ValueType = ValueType.Literal},
                new Field {Order = 14, FieldName = "CorrectionIndicator", Usage = "M", DocPosition = new Range(79, 79), Type = "N", Value = "0|1|2|3|4", ValueType = ValueType.Logical},
                new Field {Order = 15, FieldName = "ArchiveTypeIndicator", Usage = "C", DocPosition = new Range(80, 80), Type = "AN", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT26Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "26", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "CheckDetailAddendumARecordNumber", Usage = "M", DocPosition = new Range(3, 3), Type = "N", Value = "1", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "BOFDRoutingNumber", Usage = "C", DocPosition = new Range(4, 12), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 4, FieldName = "BOFDBusinessDate", Usage = "C", DocPosition = new Range(13, 20), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 5, FieldName = "BOFDItemSequenceNumber", Usage = "C", DocPosition = new Range(21, 35), Type = "NB", Value = "", ValueType = ValueType.Sequence},
                new Field {Order = 6, FieldName = "BOFDDepositAccountNumber", Usage = "C", DocPosition = new Range(36, 53), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 7, FieldName = "BOFDDepositBranch", Usage = "C", DocPosition = new Range(54, 58), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 8, FieldName = "PayeeName", Usage = "C", DocPosition = new Range(59, 73), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 9, FieldName = "TruncationIndicator", Usage = "C", DocPosition = new Range(74, 74), Type = "A", Value = "Y", ValueType = ValueType.Literal},
                new Field {Order = 10, FieldName = "BOFDConversionIndicator", Usage = "C", DocPosition = new Range(75, 75), Type = "AN", Value = "2", ValueType = ValueType.Literal},
                new Field {Order = 11, FieldName = "BOFDCorrectionIndicator", Usage = "C", DocPosition = new Range(76, 76), Type = "N", Value = "0", ValueType = ValueType.Literal},
                new Field {Order = 12, FieldName = "UserField", Usage = "C", DocPosition = new Range(77, 77), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 13, FieldName = "Reserved", Usage = "M", DocPosition = new Range(78, 80), Type = "B", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT50Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "50", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "ImageIndicator", Usage = "M", DocPosition = new Range(3, 3), Type = "N", Value = "1", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "ImageCreatorRoutingNumber", Usage = "M", DocPosition = new Range(4, 12), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 4, FieldName = "ImageCreatorDate", Usage = "M", DocPosition = new Range(13, 20), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 5, FieldName = "ImageViewFormatIndicator", Usage = "M", DocPosition = new Range(21, 22), Type = "N", Value = "0 ", ValueType = ValueType.Literal},
                new Field {Order = 6, FieldName = "ImageViewCompressionAlgorithmIdentifier", Usage = "M", DocPosition = new Range(23, 24), Type = "N", Value = "0 ", ValueType = ValueType.Literal},
                new Field {Order = 7, FieldName = "ImageViewDataSize", Usage = "C", DocPosition = new Range(25, 31), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 8, FieldName = "ViewSideIndicator", Usage = "M", DocPosition = new Range(32, 32), Type = "N", Value = "0|1", ValueType = ValueType.Logical},
                new Field {Order = 9, FieldName = "ViewDescriptor", Usage = "M", DocPosition = new Range(33, 34), Type = "N", Value = "0 ", ValueType = ValueType.Literal},
                new Field {Order = 10, FieldName = "DigitalSignatureIndicator", Usage = "M", DocPosition = new Range(35, 35), Type = "NB", Value = "0", ValueType = ValueType.Literal},
                new Field {Order = 11, FieldName = "DigitalSignatureMethod", Usage = "C", DocPosition = new Range(36, 37), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 12, FieldName = "SecurityKeySize", Usage = "C", DocPosition = new Range(38, 42), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 13, FieldName = "StartOfProtectedData", Usage = "C", DocPosition = new Range(43, 49), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 14, FieldName = "LengthOfProtectedData", Usage = "C", DocPosition = new Range(50, 56), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 15, FieldName = "ImageRecreateIndicator", Usage = "C", DocPosition = new Range(57, 57), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 16, FieldName = "UserField", Usage = "C", DocPosition = new Range(58, 65), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 17, FieldName = "Reserved", Usage = "M", DocPosition = new Range(66, 80), Type = "B", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT52Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "52", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "ECEInstitutionRoutingNumber", Usage = "M", DocPosition = new Range(3, 11), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 3, FieldName = "BatchBusinessDate", Usage = "M", DocPosition = new Range(12, 19), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 4, FieldName = "CycleNumber", Usage = "M", DocPosition = new Range(20, 21), Type = "AN", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 5, FieldName = "ECEInstitutionItemSequenceNumber", Usage = "M", DocPosition = new Range(22, 36), Type = "NB", Value = "", ValueType = ValueType.Sequence},
                new Field {Order = 6, FieldName = "SecurityOriginatorName", Usage = "C", DocPosition = new Range(37, 52), Type = "ANS", Value = "x", ValueType = ValueType.Blank},
                new Field {Order = 7, FieldName = "SecurityAuthenticator", Usage = "C", DocPosition = new Range(53, 68), Type = "ANS", Value = "v", ValueType = ValueType.Blank},
                new Field {Order = 8, FieldName = "SecurityKeyName", Usage = "C", DocPosition = new Range(69, 84), Type = "ANS", Value = "*", ValueType = ValueType.Blank},

                new Field {Order = 9, FieldName = "ClippingOrigin", Usage = "M", DocPosition = new Range(85, 85), Type = "NB", Value = "0", ValueType = ValueType.Literal},
                new Field {Order = 10, FieldName = "ClippingCoordinateH1", Usage = "C", DocPosition = new Range(86, 89), Type = "N", Value = "#", ValueType = ValueType.Blank},
                new Field {Order = 11, FieldName = "ClippingCoordinateH2", Usage = "C", DocPosition = new Range(90, 93), Type = "N", Value = "^", ValueType = ValueType.Blank},
                new Field {Order = 12, FieldName = "ClippingCoordinateV1", Usage = "C", DocPosition = new Range(94, 97), Type = "N", Value = "x", ValueType = ValueType.Blank},
                new Field {Order = 13, FieldName = "ClippingCoordinateV2", Usage = "C", DocPosition = new Range(98, 101), Type = "N", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 14, FieldName = "LengthOfImageReferenceKey", Usage = "M", DocPosition = new Range(102, 105), Type = "NB", Value = "0000", ValueType = ValueType.Literal},

                // I'm not sure at all about these yet....
                //new Field {Order = 15, FieldName = "ImageReferenceKey", Usage = "C", DocPosition = new Range(106, 110), Type = "N", Value = "52", ValueType = ValueType.Literal},
                new Field {Order = 15, FieldName = "LengthOfDigitalSignature", Usage = "M", DocPosition = new Range(106, 110), Type = "N", Value = "00000", ValueType = ValueType.Literal},
                //new Field {Order = 17, FieldName = "DigitalSignature", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "52", ValueType = ValueType.Literal},

                new Field {Order = 16, FieldName = "LengthOfImageData", Usage = "M", DocPosition = new Range(111, 117), Type = "N", Value = "", ValueType = ValueType.Length},
                // This is from start position to the end... what ever that may be
                new Field {Order = 17, FieldName = "ImageData", Usage = "M", DocPosition = new Range(118, Utils.EndOfString), Type = "Binary", Value = "", ValueType = ValueType.Binary},
            };
            return fields;
        }

        private static List<Field> BuildT61Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "61", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "AuxiliaryOnUs", Usage = "M", DocPosition = new Range(3, 17), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
                new Field {Order = 3, FieldName = "ExternalProcessingCode", Usage = "C", DocPosition = new Range(18, 18), Type = "ANS", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 4, FieldName = "PayorBankRoutingNumber", Usage = "M", DocPosition = new Range(19, 27), Type = "N", Value = "522000410", ValueType = ValueType.Literal},
                new Field {Order = 5, FieldName = "CreditAccountNumberOnUs", Usage = "C", DocPosition = new Range(28, 47), Type = "NBSM", Value = "", ValueType = ValueType.NBSM},
                new Field {Order = 6, FieldName = "ItemAccount", Usage = "M", DocPosition = new Range(48, 57), Type = "N", Value = "9999999999", ValueType = ValueType.Literal},
                new Field {Order = 7, FieldName = "ECEInstitutionItemNumber", Usage = "M", DocPosition = new Range(58, 72), Type = "NB", Value = "", ValueType = ValueType.Sequence},
                new Field {Order = 8, FieldName = "DocumentationTypeIndicator", Usage = "C", DocPosition = new Range(73, 73), Type = "AN", Value = "*", ValueType = ValueType.Blank},
                new Field {Order = 9, FieldName = "TypeOfAccountCode", Usage = "C", DocPosition = new Range(74, 74), Type = "AN", Value = "-", ValueType = ValueType.Blank},
                new Field {Order = 10, FieldName = "SourceOfWorkCode", Usage = "C", DocPosition = new Range(75, 75), Type = "AN", Value = "^", ValueType = ValueType.Blank},
                new Field {Order = 11, FieldName = "WorkType", Usage = "C", DocPosition = new Range(76, 76), Type = "AN", Value = "", ValueType = ValueType.Blank},
                new Field {Order = 12, FieldName = "DebitCreditIndicator", Usage = "C", DocPosition = new Range(77, 77), Type = "AN", Value = "#", ValueType = ValueType.Blank},
                new Field {Order = 13, FieldName = "Reserved", Usage = "M", DocPosition = new Range(78, 80), Type = "B", Value = "*", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT70Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "70", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "ItemsWithinBatchCount", Usage = "M", DocPosition = new Range(3, 6), Type = "N", Value = "0000", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "BatchTotalAmount", Usage = "M", DocPosition = new Range(7, 18), Type = "N", Value = "xxxxxxxxxxxx", ValueType = ValueType.Literal},
                new Field {Order = 4, FieldName = "MICRValidTotalAmount", Usage = "C", DocPosition = new Range(19, 30), Type = "N", Value = "9", ValueType = ValueType.Blank},
                new Field {Order = 5, FieldName = "ImagesWithinBatchCount", Usage = "C", DocPosition = new Range(31, 35), Type = "N", Value = "-", ValueType = ValueType.Blank},
                new Field {Order = 6, FieldName = "UserField", Usage = "C", DocPosition = new Range(36, 55), Type = "ANS", Value = "*", ValueType = ValueType.Blank},
                new Field {Order = 7, FieldName = "Reserved", Usage = "M", DocPosition = new Range(56, 80), Type = "B", Value = "#", ValueType = ValueType.Blank},
            };
            return fields;
        }

        private static List<Field> BuildT90Fields()
        {
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "90", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "BatchCount", Usage = "M", DocPosition = new Range(3, 8), Type = "N", Value = "999999", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "ItemsWithinCashLetterCount", Usage = "M", DocPosition = new Range(9, 16), Type = "N", Value = "0", ValueType = ValueType.Undefined},
                new Field {Order = 4, FieldName = "CashLetterTotalAmount", Usage = "M", DocPosition = new Range(17, 30), Type = "N", Value = "9", ValueType = ValueType.Undefined},
                // Docs state A, but it should be N, since it's a number
                new Field {Order = 5, FieldName = "ImagesWithinCashLetterCount", Usage = "C", DocPosition = new Range(31, 39), Type = "N", Value = "2", ValueType = ValueType.Undefined},
                new Field {Order = 6, FieldName = "CustomerName", Usage = "C", DocPosition = new Range(40, 57), Type = "A", Value = "x", ValueType = ValueType.Undefined},
                new Field {Order = 7, FieldName = "CreditDate", Usage = "C", DocPosition = new Range(58, 65), Type = "N", Value = "YYYYMMDD", ValueType = ValueType.Date},
                new Field {Order = 8, FieldName = "Reserved", Usage = "M", DocPosition = new Range(66, 80), Type = "B", Value = "", ValueType = ValueType.Blank},
            };
            return fields;
        }
    }

    public class Meta : Dictionary<Record, List<Field>>
    {
    }

    public class Record: IEquatable<Record>
    {
        public readonly string Name;
        public readonly string TypeId;

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
        // Because the documentation is 1 based and .net is 0 based...
        public Range Position => new Range(DocPosition.Start -1, DocPosition.End);
        public int Size => Position.End - Position.Start;
    }

    public struct Range: IEquatable<Range>
    {
        public readonly int Start;
        public readonly int End;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj)
        {
            return Equals((Range)obj);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() * End.GetHashCode();
        }

        public bool Equals(Range obj)
        {
            return obj.Start == Start && obj.End == End;
        }

        public static bool operator ==(Range left, Range right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range left, Range right)
        {
            return !(left == right);
        }
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
        Length,
        Binary
    }
}
