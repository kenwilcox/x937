namespace x937.Meta
{
    public class Field
    {
        public int Order { get; set; }
        public string FieldName { get; set; }
        public string Usage { get; set; }
        public Range DocPosition { private get; set; }
        //public int Size { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public FieldType FieldType { get; set; }
        // Because the documentation is 1 based and .net is 0 based...
        public Range Position => new Range(DocPosition.Start -1, DocPosition.End);
        public int Size => Position.End - Position.Start;

        public Field()
        {
            Type = "";
        }
    }
}