using System;
using System.Diagnostics;
using x937.ICL;

namespace x937
{
    public class Summary
    {
        private readonly ICLFileSummary _objFileSummary;
        private readonly ICLCashLetterSummary _objCashLetterSummary;
        private readonly ICLCreditSummary _objCreditSummary;

        private readonly ICLFileDetail _ifd;
        private ICLCashLetterDetail _icld;
        private ICLCreditDetail _icrd;

        public Summary()
        {
            // Since all the objects are internal now, not static, I don't need to null coalesce them
            _ifd = _ifd ?? new ICLFileDetail();
            _icld = _icld ?? new ICLCashLetterDetail();
            _icrd = _icrd ?? new ICLCreditDetail();
            _objFileSummary = _objFileSummary ?? new ICLFileSummary();
            _objCashLetterSummary = _objCashLetterSummary ?? new ICLCashLetterSummary();
            _objCreditSummary = _objCreditSummary ?? new ICLCreditSummary();
        }

        private void CashLetterEnd()
        {
            _objCashLetterSummary.Add(_icld);
            _icld = new ICLCashLetterDetail();
        }

        private void BundleEnd()
        {
            if (_icrd == null || _icrd.CreditAmount <= 0) return;
            _objCreditSummary.Add(_icrd);
            _icrd = new ICLCreditDetail();
        }

        public void SetSummaryData(string recType, string recData)
        {
            switch (recType)
            {
                // File Summary
                case "01":
                    _ifd.BankName = recData.Substring(36, 18);
                    _ifd.FileCreationDate = recData.Substring(23, 8);
                    _ifd.FileCreationTime = recData.Substring(31, 4);
                    break;
                case "99":
                    _ifd.TotalFileAmount = Convert.ToInt64(recData.Substring(24, 16));
                    _ifd.TotalNumberofRecords = Convert.ToInt32(recData.Substring(8, 8));
                    break;

                // Cash Letter Summary
                case "10":
                    _icld.CashLetterPosition = Convert.ToInt32(recData.Substring(44, 8));
                    break;
                case "90":
                    _icld.CashLetterAmount = Convert.ToInt64(recData.Substring(16, 14));
                    CashLetterEnd();
                    break;

                // Bundle Summary
                case "20": break;
                case "70":
                    BundleEnd();
                    break;

                // Credit Summary
                case "61":
                    _icrd.CreditAmount = Convert.ToInt64(recData.Substring(48, 10));
                    _icrd.PostingAccRT = recData.Substring(19, 9);
                    _icrd.PostingAccBankOnUs = recData.Substring(28, 20);
                    break;
                //default:
                //    Debug.WriteLine($"Do we care about {recType}?");
                //    break;
            }
        }

        public TreeNode<string> CreateNodeValues()
        {
            Debug.WriteLine("CreateNodeValues");
            Console.WriteLine("**SUMMARY**");
            var tvSum = new TreeNode<string>(_ifd.BankName.Trim() + " - ICL File Summary");
            var total = tvSum.AddChild("File Total Amount ($) - " + _ifd.TotalFileAmount);
            total.AddChild("File Creation Date - " + _ifd.FileCreationDate);
            total.AddChild("Total No of Records - " + _ifd.TotalNumberofRecords);
            total.AddChild("Total No of Cash Letter - " + _objCashLetterSummary.Count);
            var cl = tvSum.AddChild("Total Cash Letter Amount ($) - " + _objCashLetterSummary.TotalCashLetterAmount);
            foreach (var icd in _objCashLetterSummary)
            {
                cl.AddChild("Cash Letter " + icd.CashLetterPosition + " Amount ($) - " + icd.CashLetterAmount);
            }
            if (_objCreditSummary == null || _objCreditSummary.Count <= 0) return tvSum;
            cl.AddChild("Total No of Credit Records - " + _objCreditSummary.Count);
            var credit = tvSum.AddChild("Total Credit Amount ($) - " + _objCreditSummary.TotalCreditRecordAmount);
            foreach (ICLCreditDetail crd in _objCreditSummary)
            {
                credit.AddChild($"Posting Bank On-Us {crd.PostingAccBankOnUs.Trim()} Amount ($)- {crd.CreditAmount}");
            }

            return tvSum;
        }

        public static Summary GetSummary(X9Recs records)
        {
            var summary = new Summary();
            foreach(X9Rec item in records)
            {
                summary.SetSummaryData(item.RecType, item.RecData);
            }
            return summary;
        }
    }
}
