using System.Linq;

namespace x937.ICL
{
    public sealed class ICLCashLetterSummary : ICLBase<ICLCashLetterDetail>
    {
        public long TotalCashLetterAmount
        {
            get
            {
                return this.Aggregate(0, (current, cld) => (int) (current + cld.CashLetterAmount));
            }
        }
    }
}