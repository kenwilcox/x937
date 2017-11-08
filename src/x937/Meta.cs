using System;
using System.Collections.Generic;
using System.Text;

namespace x937
{
    public class Builder
    {
        public static Meta GetMeta()
        {
            var meta = new Meta();
            var t01 = BuildT01Record();
            meta.Add(t01.Record, t01.Fields);
            var t10 = BuildT10Record();
            meta.Add(t10.Record, t10.Fields);
            return meta;
        }

        private static (Record Record, List<Field> Fields) BuildT01Record()
        {
            var record = new Record("FileHeaderRecord", "01");
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "01", ValueType = ValueType.Literal},
                new Field {Order = 2, FieldName = "StandardLevel", Usage = "M", DocPosition = new Range(3, 4), Type = "N", Value = "03", ValueType = ValueType.Literal},
                new Field {Order = 3, FieldName = "TestFileIndicator", Usage = "M", DocPosition = new Range(5, 5), Type = "A", Value = "T|P", ValueType = ValueType.Logical},
                new Field {Order = 4, FieldName = "ImmediateDestinationRoutingNumber", Usage = "M", DocPosition = new Range(6, 14), Type = "N", Value = "TTTTAAAAC", ValueType = ValueType.RoutePattern},
                new Field {Order = 5, FieldName = "ImmediateOriginRoutingNumber", Usage = "M", DocPosition = new Range(15, 23), Type = "N", Value = "TTTTAAAACC", ValueType = ValueType.RoutePattern},
                new Field {Order = 6, FieldName = "FileCreationDate", Usage = "M", DocPosition = new Range(24, 31), Type = "N", Value = "YYMMDD", ValueType = ValueType.Date},
                new Field {Order = 7, FieldName = "FileCreationTime", Usage = "M", DocPosition = new Range(32, 35), Type = "N", Value = "HHMM", ValueType = ValueType.Date},
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
            return (record, fields);
        }

        private static (Record Record, List<Field>Fields) BuildT10Record()
        {
            var record = new Record("CashLetterHeaderRecord", "10");
            var fields = new List<Field>
            {
                new Field {Order = 1, FieldName = "RecordType", Usage = "M", DocPosition = new Range(1, 2), Type = "N", Value = "10", ValueType = ValueType.Literal},
            };
            return (record, fields);
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
        //Pattern,
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
