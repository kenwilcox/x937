namespace x937
{
    public class X9Rec
    {
        //public X9Rec()
        //{

        //}

        public X9Rec(string recType, string recData, string recImage)
        {
            RecType = recType;
            RecData = recData;
            RecImage = recImage;
        }

        public string RecType { get; set; }

        public string RecData { get; set; }

        public string RecImage { get; set; }
    }
}