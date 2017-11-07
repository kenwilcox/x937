namespace x937
{
    public class X9Rec
    {
        public X9Rec()
        {
            RecType = string.Empty;
            RecData = string.Empty;
            RecImage = string.Empty;
            CheckImageType = CheckImage.None;
        }

        public X9Rec(string recType, string recData, string recImage): this()
        {
            RecType = recType;
            RecData = recData;
            RecImage = recImage;
        }

        public X9Rec(string recType, string recData, string recImage, CheckImage imageType) :
            this(recType, recData, recImage)
        {
            CheckImageType = imageType;
        }

        public string RecType { get; set; }

        public string RecData { get; set; }

        public string RecImage { get; set; }

        public string FilePath { get; set; }
        public CheckImage CheckImageType { get; set; }
    }

    public enum CheckImage
    {
        None,
        Front,
        Back
    }
}
