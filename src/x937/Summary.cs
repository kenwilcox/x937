using System;
using System.Diagnostics;
using x937.ICL;

namespace x937
{
    public class Summary
    {
        //public readonly ICLFileSummary FileSummary;
        public readonly ICLCashLetterSummary CashLetterSummary;
        public readonly ICLCreditSummary CreditSummary;

        public readonly ICLFileDetail FileDetail;
        public ICLCashLetterDetail CashLetterDetail;
        public ICLCreditDetail CreditDetail;

        public Summary()
        {
            FileDetail = new ICLFileDetail();
            CashLetterDetail = new ICLCashLetterDetail();
            CreditDetail = new ICLCreditDetail();
            //FileSummary = new ICLFileSummary();
            CashLetterSummary = new ICLCashLetterSummary();
            CreditSummary = new ICLCreditSummary();
        }

        private void CashLetterEnd()
        {
            CashLetterSummary.Add(CashLetterDetail);
            CashLetterDetail = new ICLCashLetterDetail();
        }

        private void BundleEnd()
        {
            if (CreditDetail.CreditAmount <= 0) return;
            CreditSummary.Add(CreditDetail);
            CreditDetail = new ICLCreditDetail();
        }

        public void SetSummaryData(string recType, string recData)
        {
            switch (recType)
            {
                // File Summary
                case "01":
                    FileDetail.BankName = recData.Substring(36, 18);
                    FileDetail.FileCreationDate = recData.Substring(23, 8);
                    FileDetail.FileCreationTime = recData.Substring(31, 4);
                    break;
                case "99":
                    FileDetail.TotalFileAmount = Convert.ToInt64(recData.Substring(24, 16));
                    FileDetail.TotalNumberofRecords = Convert.ToInt32(recData.Substring(8, 8));
                    break;

                // Cash Letter Summary
                case "10":
                    CashLetterDetail.CashLetterPosition = Convert.ToInt32(recData.Substring(44, 8));
                    break;
                case "90":
                    CashLetterDetail.CashLetterAmount = Convert.ToInt64(recData.Substring(16, 14));
                    CashLetterEnd();
                    break;

                // Bundle Summary
                case "20": break;
                case "70":
                    BundleEnd();
                    break;

                // Credit Summary
                case "61":
                    CreditDetail.CreditAmount = Convert.ToInt64(recData.Substring(48, 10));
                    CreditDetail.PostingAccRt = recData.Substring(19, 9);
                    CreditDetail.PostingAccBankOnUs = recData.Substring(28, 20);
                    break;
                default:
                    Debug.WriteLine($"Do we care about {recType}?");
                    break;
            }
        }

        public TreeNode<string> CreateNodeValues()
        {
            Debug.WriteLine("CreateNodeValues");
            Console.WriteLine("**SUMMARY**");
            var tvSum = new TreeNode<string>(FileDetail.BankName.Trim() + " - ICL File Summary");
            var total = tvSum.AddChild("File Total Amount ($) - " + FileDetail.TotalFileAmount);
            total.AddChild("File Creation Date - " + FileDetail.FileCreationDate);
            total.AddChild("Total No of Records - " + FileDetail.TotalNumberofRecords);
            total.AddChild("Total No of Cash Letter - " + CashLetterSummary.Count);
            var cl = tvSum.AddChild("Total Cash Letter Amount ($) - " + CashLetterSummary.TotalCashLetterAmount);
            foreach (var icd in CashLetterSummary)
            {
                cl.AddChild("Cash Letter " + icd.CashLetterPosition + " Amount ($) - " + icd.CashLetterAmount);
            }
            if (CreditSummary.Count <= 0) return tvSum;
            cl.AddChild("Total No of Credit Records - " + CreditSummary.Count);
            var credit = tvSum.AddChild("Total Credit Amount ($) - " + CreditSummary.TotalCreditRecordAmount);
            foreach (ICLCreditDetail crd in CreditSummary)
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
