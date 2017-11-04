using System.Linq;

namespace x937.ICL
{
    public sealed class ICLCreditSummary : ICLBase<ICLCreditDetail>
    {
        public long TotalCreditRecordAmount
        {
            get
            {
                return this.Sum(cd => cd.CreditAmount);
            }
        }
    }
}
