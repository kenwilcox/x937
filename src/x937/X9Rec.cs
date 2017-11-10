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
            ImageData = null;
        }

        public X9Rec(string recType, string recData, string recImage): this()
        {
            RecType = recType;
            RecData = recData;
            RecImage = recImage;
        }

        public X9Rec(string recType, string recData, string recImage, CheckImage imageType, byte[] imageData) :
            this(recType, recData, recImage)
        {
            CheckImageType = imageType;
            ImageData = imageData;
        }

        public string RecType { get; set; }

        public string RecData { get; set; }

        public string RecImage { get; set; }

        public string FilePath { get; set; }
        public CheckImage CheckImageType { get; set; }
        public byte[] ImageData { get; set; }
    }

    public enum CheckImage
    {
        None,
        Front,
        Back
    }
}
