namespace x937.ICL
{
    public class ICLCreditDetail
    {
        public string PostingAccRt { get; set; }
        public string PostingAccBankOnUs { get; set; }
        public long CreditAmount { get; set; }

        public ICLCreditDetail()
        {
            PostingAccRt = "";
            PostingAccBankOnUs = "";
        }
    }
}
